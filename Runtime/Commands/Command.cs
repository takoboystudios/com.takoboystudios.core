using System.Collections.Generic;

namespace TakoBoyStudios.Commands
{
	/// <summary>
	/// One step of a sequence that takes time to show.
	///
	/// The point of the pattern is that the queue does not advance until a command says it
	/// has finished, so the thing driving it can be an animation rather than a frame count.
	/// A move slides, a hit flashes and shakes, a death fades -- and whatever comes next
	/// waits, without anyone writing a coroutine or a timer.
	///
	/// Generic over the context so this can live here rather than in a game: the context is
	/// whatever a command needs to do its work -- a view, a renderer, a board -- and the
	/// queue only passes it through.
	/// </summary>
	public abstract class Command<TContext>
	{
		public enum State
		{
			Pending,
			Running,
			Finished
		}

		public State state { get; private set; }

		/// <summary>Shown by a debug drawer. Override for something more use than the type name.</summary>
		public virtual string Label => GetType().Name;

		/// <summary>Called once, when the command reaches the front of the queue.</summary>
		public virtual void Begin(TContext context) { }

		/// <summary>
		/// Called every frame while running. Call <see cref="Finish"/> when the thing being
		/// waited on is over; a command that never does will stall the queue, which is the
		/// intended failure -- a silently skipped animation is harder to notice.
		/// </summary>
		public virtual void Update(TContext context, float deltaTime) { }

		protected void Finish() => state = State.Finished;

		internal void MarkRunning() => state = State.Running;
	}

	/// <summary>
	/// Runs commands one at a time, in order, each waiting for the last.
	///
	/// Deliberately not a coroutine: the queue is inspectable. You can ask what is running,
	/// what has been, and what is still to come, which is what makes a debug drawer for it
	/// worth having.
	/// </summary>
	public sealed class CommandQueue<TContext>
	{
		readonly List<Command<TContext>> m_commands = new List<Command<TContext>>();
		int m_current;

		public IReadOnlyList<Command<TContext>> commands => m_commands;
		public int current => m_current;

		/// <summary>True while anything is left to run. Gate input on this.</summary>
		public bool IsRunning => m_current < m_commands.Count;

		public void Add(Command<TContext> command)
		{
			if (command != null) m_commands.Add(command);
		}

		public void Process(TContext context, float deltaTime)
		{
			// A command may queue more work as it finishes, so this walks forward rather
			// than caching the count.
			while (m_current < m_commands.Count)
			{
				var command = m_commands[m_current];
				if (command == null) { m_current++; continue; }

				if (command.state == Command<TContext>.State.Pending)
				{
					command.MarkRunning();
					command.Begin(context);
				}

				if (command.state == Command<TContext>.State.Running)
				{
					command.Update(context, deltaTime);
				}

				if (command.state != Command<TContext>.State.Finished) return;

				m_current++;

				// One finished command should not let the next one's Begin and Update run
				// in the same frame with the same delta -- a zero-length command would then
				// drain the whole queue in a frame and nothing would be seen.
				return;
			}
		}

		/// <summary>Forget everything. For a restart, or a scene change mid-sequence.</summary>
		public void Clear()
		{
			m_commands.Clear();
			m_current = 0;
		}
	}
}

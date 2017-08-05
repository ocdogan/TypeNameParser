namespace TypeNameResolver
{
	#region IStringView

	public interface IStringView
	{
		int Start { get; set; }
		int End { get; set; }
		string Text { get; }
		bool IsEmpty { get; }
		bool IsLocked { get; }
		void Lock();
	}

	#endregion IStringView
}

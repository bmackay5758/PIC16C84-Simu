﻿
namespace PICSimulator.Model.Commands
{
	abstract class PICCommand
	{
		public readonly string SourceCodeText;	// Line in the src file
		public readonly uint SourceCodeLine;	// LineNmbr in the src file

		public readonly uint Position;
		public readonly uint Command;

		public PICCommand(string sct, uint scl, uint pos, uint cmd)
		{
			SourceCodeText = sct;
			SourceCodeLine = scl;
			Position = pos;
			Command = cmd;
		}

		public override string ToString()
		{
			return string.Format("[{0:X04}] <{1:X04}> ({2}: {3})", Position, Command, SourceCodeLine, SourceCodeText);
		}
	}
}
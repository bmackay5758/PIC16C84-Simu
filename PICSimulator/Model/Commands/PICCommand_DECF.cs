﻿
namespace PICSimulator.Model.Commands
{
	class PICCommand_DECF : PICCommand
	{
		public const string COMMANDCODE = "00 0011 dfff ffff";

		public readonly bool Target;
		public readonly uint Register;

		public PICCommand_DECF(string sct, uint scl, uint pos, uint cmd)
			: base(sct, scl, pos, cmd)
		{
			Target = Parameter.GetBoolParam('d').Value;
			Register = Parameter.GetParam('f').Value;
		}

		public override void Execute(PICController controller)
		{
			uint Result = controller.GetBankedRegister(Register);

			if (Result == 0)
				Result = 0xFF;
			else
				Result -= 1;

			controller.SetUnbankedRegisterBit(PICMemory.ADDR_STATUS, PICMemory.STATUS_BIT_Z, Result == 0);

			if (Target)
				controller.SetBankedRegister(Register, Result);
			else
				controller.SetWRegister(Result);

		}

		public override string GetCommandCodeFormat()
		{
			return COMMANDCODE;
		}

		public override uint GetCycleCount(PICController controller)
		{
			return 1;
		}
	}
}

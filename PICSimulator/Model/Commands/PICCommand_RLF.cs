﻿
namespace PICSimulator.Model.Commands
{
	/// <summary>
	/// The contents of register 'f' are rotated
	/// one bit to the left through the Carry
	/// Flag. If 'd' is 0 the result is placed in the
	/// W register. If 'd' is 1 the result is stored
	/// back in register 'f'.
	/// </summary>
	class PICCommand_RLF : PICCommand
	{
		public const string COMMANDCODE = "00 1101 dfff ffff";

		public readonly bool Target;
		public readonly uint Register;

		public PICCommand_RLF(string sct, uint scl, uint pos, uint cmd)
			: base(sct, scl, pos, cmd)
		{
			Target = Parameter.GetBoolParam('d').Value;
			Register = Parameter.GetParam('f').Value;
		}

		public override void Execute(PICController controller)
		{
			uint Result = controller.GetBankedRegister(Register);

			uint Carry_Old = controller.GetUnbankedRegisterBit(PICMemory.ADDR_STATUS, PICMemory.STATUS_BIT_C) ? 1u : 0u;
			uint Carry_New = (Result & 0x80) >> 7;

			Result = Result << 1;
			Result &= 0xFF;

			Result |= Carry_Old;

			controller.SetUnbankedRegisterBit(PICMemory.ADDR_STATUS, PICMemory.STATUS_BIT_C, Carry_New != 0);

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

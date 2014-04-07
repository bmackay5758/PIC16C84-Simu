﻿
using PICSimulator.Helper;
namespace PICSimulator.Model
{
	class PICTimer
	{
		private bool prev_RA4 = false;

		public PICTimer()
		{

		}

		public void Update(PICController controller)
		{
			bool tmr_mode = controller.GetRegisterBit(PICController.ADDR_OPTION, PICController.OPTION_BIT_T0CS);
			bool edge_mode = controller.GetRegisterBit(PICController.ADDR_OPTION, PICController.OPTION_BIT_T0SE);

			if (tmr_mode)
			{
				bool curr_A4 = controller.GetRegisterBit(PICController.ADDR_PORT_A, 4);

				if (edge_mode)
				{
					if (!prev_RA4 && curr_A4)
					{
						Inc(controller);
					}
				}
				else
				{
					if (prev_RA4 && !curr_A4)
					{
						Inc(controller);
					}
				}
			}
			else
			{
				Inc(controller);
			}

			prev_RA4 = controller.GetRegisterBit(PICController.ADDR_PORT_A, 4);
		}

		private void Inc(PICController controller)
		{
			uint current = controller.GetRegister(PICController.ADDR_TMR0);

			uint scale = GetPreScale(controller);

			uint Result = current + scale;
			if (Result > 0xFF)
			{
				// TODO Interrupt
			}

			Result %= 0x100;

			controller.SetRegister(PICController.ADDR_TMR0, Result);
		}

		private uint GetPreScale(PICController controller)
		{
			bool prescale_mode = controller.GetRegisterBit(PICController.ADDR_OPTION, PICController.OPTION_BIT_PSA);

			uint scale = 0;
			scale += controller.GetRegisterBit(PICController.ADDR_OPTION, PICController.OPTION_BIT_PS2) ? 1U : 0U;
			scale *= 2;
			scale += controller.GetRegisterBit(PICController.ADDR_OPTION, PICController.OPTION_BIT_PS1) ? 1U : 0U;
			scale *= 2;
			scale += controller.GetRegisterBit(PICController.ADDR_OPTION, PICController.OPTION_BIT_PS0) ? 1U : 0U;

			return prescale_mode ? 1 : (BinaryHelper.SHL(2, scale));
		}

		private uint UIntPower(uint x, uint power)
		{
			if (power == 0)
				return 1;
			if (power == 1)
				return x;
			// ----------------------
			int n = 15;
			while ((power <<= 1) >= 0)
				n--;

			uint tmp = x;
			while (--n > 0)
				tmp = tmp * tmp *
					 (((power <<= 1) < 0) ? x : 1);
			return tmp;
		}
	}
}
﻿using PICSimulator.Helper;
using System;
using System.Collections.Generic;

namespace PICSimulator.Model
{
	class PICMemory
	{
		public const uint ADDR_INDF = 0x00;			// TODO Add indirect Adressing with INDF and FSR (Kap 4.5)
		public const uint ADDR_TMR0 = 0x01;
		public const uint ADDR_PCL = 0x02;
		public const uint ADDR_STATUS = 0x03;
		public const uint ADDR_FSR = 0x04;
		public const uint ADDR_PORT_A = 0x05;
		public const uint ADDR_PORT_B = 0x06;
		public const uint ADDR_PCLATH = 0x0A;
		public const uint ADDR_INTCON = 0x0B;

		public const uint ADDR_OPTION = 0x81;
		public const uint ADDR_TRIS_A = 0x85;
		public const uint ADDR_TRIS_B = 0x86;
		public const uint ADDR_EECON1 = 0x88;
		public const uint ADDR_EECON2 = 0x89;

		public const uint STATUS_BIT_IRP = 7;		// Unused in PIC16C84
		public const uint STATUS_BIT_RP0 = 5;		// Register Bank Selection Bit //TODO Bank Select
		public const uint STATUS_BIT_TO = 4;		// Time Out Bit
		public const uint STATUS_BIT_PD = 3;		// Power Down Bit
		public const uint STATUS_BIT_Z = 2;			// Zero Bit
		public const uint STATUS_BIT_DC = 1;		// Digit Carry Bit
		public const uint STATUS_BIT_C = 0;			// Carry Bit

		public const uint OPTION_BIT_RBPU = 7;		// PORT-B Pull-Up Enable Bit
		public const uint OPTION_BIT_INTEDG = 6;	// Interrupt Edge Select Bit
		public const uint OPTION_BIT_T0CS = 5;		// TMR0 Clock Source Select Bit
		public const uint OPTION_BIT_T0SE = 4;		// TMR0 Source Edge Select Bit
		public const uint OPTION_BIT_PSA = 3;		// Prescaler Alignment Bit
		public const uint OPTION_BIT_PS2 = 2;		// Prescaler Rate Select Bit [2]
		public const uint OPTION_BIT_PS1 = 1;		// Prescaler Rate Select Bit [1]
		public const uint OPTION_BIT_PS0 = 0;		// Prescaler Rate Select Bit [0]

		public const uint INTCON_BIT_GIE = 7;		// Global Interrupt Enable Bit
		public const uint INTCON_BIT_EEIE = 6;		// EE Write Complete Interrupt Enable Bit
		public const uint INTCON_BIT_T0IE = 5;		// TMR0 Overflow Interrupt Enable Bit
		public const uint INTCON_BIT_INTE = 4;		// RB0/INT Interrupt Bit
		public const uint INTCON_BIT_RBIE = 3;		// RB Port Change Interrupt Enable Bit
		public const uint INTCON_BIT_T0IF = 2;		// TMR0 Overflow Interrupt Flag Bit
		public const uint INTCON_BIT_INTF = 1;		// RB0/INT Interrupt Flag Bit
		public const uint INTCON_BIT_RBIF = 0;		// RB Port Change Interrupt Flag Bit

		public delegate uint RegisterRead(uint Pos);
		public delegate void RegisterWrite(uint Pos, uint Value);

		private readonly Dictionary<uint, Tuple<RegisterRead, RegisterWrite>> SpecialRegisterEvents;

		private uint[] register = new uint[0xFF];

		public PICMemory()
		{
			SpecialRegisterEvents = new Dictionary<uint, Tuple<RegisterRead, RegisterWrite>>() 
			{
				#region Linked Register

				//##############################################################################
				{
					ADDR_INDF,	
					Tuple.Create<RegisterRead, RegisterWrite>(
						GetRegisterDirect, 
						(p, v) => { SetRegisterDirect(p, v); SetRegisterDirect(p+0x80, v); })
				}, 
				{
					ADDR_INDF+0x80,	
					Tuple.Create<RegisterRead, RegisterWrite>(
						GetRegisterDirect, 
						(p, v) => { SetRegisterDirect(p, v); SetRegisterDirect(p-0x80, v); })
				}, 
				//##############################################################################
				{
					ADDR_PCL, 
					Tuple.Create<RegisterRead, RegisterWrite>(
						GetRegisterDirect, 
						(p, v) => { SetRegisterDirect(p, v); SetRegisterDirect(p+0x80, v); })
				}, 
				{
					ADDR_PCL + 0x80, 
					Tuple.Create<RegisterRead, RegisterWrite>(
						GetRegisterDirect, 
						(p, v) => { SetRegisterDirect(p, v); SetRegisterDirect(p-0x80, v); })
				}, 
				//##############################################################################
				{
					ADDR_STATUS, 
					Tuple.Create<RegisterRead, RegisterWrite>(
						GetRegisterDirect, 
						(p, v) => { SetRegisterDirect(p, v); SetRegisterDirect(p+0x80, v); })
				}, 
				{
					ADDR_STATUS + 0x80, 
					Tuple.Create<RegisterRead, RegisterWrite>(
						GetRegisterDirect, 
						(p, v) => { SetRegisterDirect(p, v); SetRegisterDirect(p-0x80, v); })
				}, 
				//##############################################################################
				{
					ADDR_FSR, 
					Tuple.Create<RegisterRead, RegisterWrite>(
						GetRegisterDirect, 
						(p, v) => { SetRegisterDirect(p, v); SetRegisterDirect(p+0x80, v); })
				}, 
				{
					ADDR_FSR + 0x80, 
					Tuple.Create<RegisterRead, RegisterWrite>(
						GetRegisterDirect, 
						(p, v) => { SetRegisterDirect(p, v); SetRegisterDirect(p-0x80, v); })
				}, 
				//##############################################################################
				{
					ADDR_PCLATH, 
					Tuple.Create<RegisterRead, RegisterWrite>(
						GetRegisterDirect, 
						(p, v) => { SetRegisterDirect(p, v); SetRegisterDirect(p+0x80, v); })
				}, 
				{
					ADDR_PCLATH + 0x80, 
					Tuple.Create<RegisterRead, RegisterWrite>(
						GetRegisterDirect, 
						(p, v) => { SetRegisterDirect(p, v); SetRegisterDirect(p-0x80, v); })
				}, 
				//##############################################################################
				{
					ADDR_INTCON, 
					Tuple.Create<RegisterRead, RegisterWrite>(
						GetRegisterDirect, 
						(p, v) => { SetRegisterDirect(p, v); SetRegisterDirect(p+0x80, v); })
				},
				{
					ADDR_INTCON + 0x80, 
					Tuple.Create<RegisterRead, RegisterWrite>(
						GetRegisterDirect, 
						(p, v) => { SetRegisterDirect(p, v); SetRegisterDirect(p-0x80, v); })
				},
				#endregion
			};
		}

		#region Getter/Setter

		public uint GetRegister(uint p)
		{
			if (SpecialRegisterEvents.ContainsKey(p))
			{
				return SpecialRegisterEvents[p].Item1(p);
			}
			else
			{
				return GetRegisterDirect(p);
			}
		}

		public void SetRegister(uint p, uint n)
		{
			if (SpecialRegisterEvents.ContainsKey(p))
			{
				SpecialRegisterEvents[p].Item2(p, n);
			}
			else
			{
				SetRegisterDirect(p, n);
			}
		}

		protected uint GetRegisterDirect(uint p)
		{
			return register[p];
		}

		protected void SetRegisterDirect(uint p, uint n)
		{
			n %= 0x100; // Just 4 Safety

			register[p] = n;
		}

		public void SetRegisterBit(uint p, uint bitpos, bool newVal)
		{
			SetRegister(p, BinaryHelper.SetBit(GetRegister(p), bitpos, newVal));
		}

		public bool GetRegisterBit(uint p, uint bitpos)
		{
			return BinaryHelper.GetBit(GetRegister(p), bitpos);
		}

		#endregion

		public void HardResetRegister()
		{
			for (uint i = 0; i < 0xFF; i++)
			{
				SetRegister(i, 0x00);
			}

			SetRegister(ADDR_STATUS, 0x18);
			SetRegister(ADDR_OPTION, 0xFF);
			SetRegister(ADDR_TRIS_A, 0x1F);
			SetRegister(ADDR_TRIS_B, 0xFF);
		}

		public void SoftResetRegister()
		{
			SetRegister(ADDR_PCL, 0x00);
			SetRegister(ADDR_PCLATH, 0x00);
			SetRegister(ADDR_INTCON, (GetRegister(ADDR_INTCON) & 0x01));
			SetRegister(ADDR_OPTION, 0xFF);
			SetRegister(ADDR_TRIS_A, 0x1F);
			SetRegister(ADDR_TRIS_B, 0xFF);
			SetRegister(ADDR_EECON1, (GetRegister(ADDR_EECON1) & 0x08));
		}
	}
}

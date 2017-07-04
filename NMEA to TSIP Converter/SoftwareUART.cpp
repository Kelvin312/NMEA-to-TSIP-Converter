/*
* SoftwareUART.cpp
*
* Created: 02.07.2017 16:20:24
*  Author: Kelvin
*/

#include "stdafx.h"

class SoftUart
{
	public:
	SoftUart(volatile u8 &rxPort, u8 rxPin, volatile u8 &txPort, u8 txPin):
	rxPort(rxPort),
	rxPin(rxPin),
	txPort(txPort),
	txPin(txPin)
	{
		txFrameCounter = txFrameSize;
		transmitComplete = true;
		rxFlag = WaitStartBit;
	}
	private:
	volatile u8 &rxPort;
	volatile u8 &txPort;
	const u8 rxPin, txPin;
	
	inline u8 GetRxPin(){ return rxPort & _BV(rxPin); }
	inline void SetTxPinHigh(){ txPort |=_BV(txPin); }
	inline void SetTxPinLow(){ txPort &= ~_BV(txPin); }
	
	enum
	{
		WaitStartBit,
		ReadData,
		WaitStopBit
	} rxFlag;
	
	u8 timerRxCtr, rxFrameCounter;
	u8 rxFrameBuffer, rxMask;
	static const u8 rxFrameDataBits = 8; //Количество бит данных
	
	bool transmitComplete;
	u8 timerTxCtr, txFrameCounter;
	u16 txFrameBuffer;
	static const u8 txFrameSize = 10; //Количество бит данных + стоп и старт бит
	
	public:
	
	inline bool RxProcessing(u8 &data)
	{
		switch(rxFlag)
		{
			case WaitStartBit:
			if(GetRxPin() == 0)
			{
				timerRxCtr = 4;
				rxFlag = ReadData;
				rxFrameCounter = rxFrameDataBits;
				rxFrameBuffer = 0;
				rxMask = 1;
			}
			break;
			case ReadData: //Принимаем 8 бит
			if(--timerRxCtr == 0)
			{
				timerRxCtr = 3;
				if(GetRxPin())
				{
					rxFrameBuffer |= rxMask;
				}
				rxMask <<= 1;
				if(--rxFrameCounter == 0)
				{
					rxFlag = WaitStopBit;
				}
			}
			break;
			case WaitStopBit:
			if(--timerRxCtr == 0)
			{
				timerRxCtr = 3;
				if(GetRxPin())
				{
					rxFlag = WaitStartBit;
					data = rxFrameBuffer;
					return true;
				}
			}
			break;
		}
		return false;
	}

	inline void Transmit(u8 data)
	{
		if(transmitComplete)
		{
			txFrameBuffer = (((u16)data)<<1) | 0x300; //Добавляем старт и стоп биты.
			txFrameCounter = txFrameSize;
			transmitComplete = false;
		}
	}

	inline bool TxProcessing()
	{
		if(transmitComplete) return true;
		if(--timerTxCtr == 0)
		{
			timerTxCtr = 3;
			if(txFrameBuffer & 1)
			{
				SetTxPinHigh();
			}
			else
			{
				SetTxPinLow();
			}
			txFrameBuffer >>= 1;
			if(--txFrameCounter == 0)
			{
				SetTxPinHigh();
				transmitComplete = true;
				return transmitComplete;
			}
			
		}
		return false;
	}

};
#ifndef SOFT_UART_H_
#define SOFT_UART_H_
#include "stdafx.h"

class SoftUart
{
	private:
	volatile u8 &rxPort;
	volatile u8 &txPort;
	const u8 rxPin, txPin;
	const ParityAndStop mode;
	
	inline u8 GetRxPin(){ return rxPort & rxPin; }
	inline void SetTxPinHigh(){ txPort |= txPin; }
	inline void SetTxPinLow(){ txPort &= ~txPin; }
	
	enum
	{
		WaitStartBit,
		ReadData,
		WaitStopBit
	} rxFlag;
	
	u8 timerRxCtr, rxFrameCounter;
	u8 rxFrameBuffer, rxMask, rxParityBit;
	const u8 rxFrameDataBits = (mode == ParityAndStop::None1) ? 8 : 9; //Количество бит данных
	
	volatile bool transmitComplete;
	u8 timerTxCtr;
	volatile u8 txFrameCounter, txParityBit;
	volatile u16 txFrameBuffer;
	const u8 txFrameSize = (mode == ParityAndStop::None1) ? 10 : 11; //Количество бит данных + стоп и старт бит
	
	public:
	SoftUart(volatile u8 &rxPort, u8 rxPin, volatile u8 &txPort, u8 txPin, ParityAndStop mode = ParityAndStop::None1):
	rxPort(rxPort),
	rxPin(rxPin),
	txPort(txPort),
	txPin(txPin),
	mode(mode)
	{
		txFrameCounter = txFrameSize;
		transmitComplete = true;
		rxFlag = WaitStartBit;
	}
	
	bool RxProcessing(u8 &data)
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
				rxParityBit = (mode == ParityAndStop::Odd1) ? 1:0;
			}
			break;
			case ReadData: //Принимаем 8 бит
			if(--timerRxCtr == 0)
			{
				timerRxCtr = 3;
				if(GetRxPin())
				{
					rxFrameBuffer |= rxMask;
					rxParityBit ^= 1;
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
					if((mode == ParityAndStop::Odd1 || mode == ParityAndStop::Even1) 
					&& rxParityBit) return false;
					data = rxFrameBuffer;
					return true;
				}
			}
			break;
		}
		return false;
	}

	void Transmit(u8 data)
	{
		if(transmitComplete)
		{
			txParityBit = data ^ (data >> 4);
			txParityBit ^= txParityBit >> 2;
			txParityBit ^= txParityBit >> 1;
			if(mode == ParityAndStop::Odd1) txParityBit ^= 1;
			txFrameBuffer = (u16(data)<<1) | 0xC00; //Добавляем старт и стоп биты.
			if(mode == ParityAndStop::None1 || (txParityBit & 1)) txFrameBuffer |= 1<<9;
			txFrameCounter = txFrameSize;
			transmitComplete = false;
		}
	}

	bool TxProcessing()
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
	
	void WaitAndTransmit(u8 data)
	{
		while(!transmitComplete);
		Transmit(data);
	}

};

#endif /* SOFT_UART_H_ */
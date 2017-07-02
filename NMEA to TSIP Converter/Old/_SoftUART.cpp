/*
 * SoftUART.cpp
 *
 * Created: 28.06.2017 7:36:17
 *  Author: Andrey
 */ 

#include "stdafx.h"
#include "_IUart.h"

template<u8 rxBufferSize, u8 txBufferSize>
class _SoftUart: public _IUart
{
	private:
	volatile u8 rxWriteIndex, rxReadIndex, rxCounter;
	volatile u8 rxBuffer[rxBufferSize];
	volatile u8 txWriteIndex, txReadIndex, txCounter;
	volatile bool txEnable, isTerminateMsg;
	volatile u8 txBuffer[txBufferSize];

	volatile enum
	{
		RxDisable,
		WaitStartBit,
		ReadData,
		WaitStopBit
	} rxFlag;
	
	u8 &rxPort;
	const u8 rxPin;
	u8 &txPort;
	const u8 txPin;
	inline u8 GetRxPin(){ return rxPort & (1<<rxPin); }
	inline void SetTxPinHigh(){ txPort |= 1<<txPin; }
	inline void SetTxPinLow(){ txPort &= ~(1<<txPin); }
	
	u8 rxFrameCounter, timerRxCtr;
	u8 rxFrameBuffer, rxMask;
	static const u8 rxFrameDataBits = 8; //Количество бит данных
	const u8 terminateKey;
	
	u8 timerTxCtr, txFrameCounter;
	u16 txFrameBuffer;
	static const u8 txFrameSize = 10; //Количество бит данных + стоп и старт бит
	
	public:
	SoftUart(u8 &rxPort, u8 rxPin, u8 &txPort, u8 txPin, u8 terminateKey):
	rxPort(rxPort),
	rxPin(rxPin),
	txPort(txPort),
	txPin(txPin),
	terminateKey(terminateKey)
	{
		txFrameCounter = txFrameSize;
	}
	
	void RxProcessing()
	{
		switch(rxFlag)
		{
			case RxDisable:
			break;
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
					rxBuffer[rxWriteIndex] = rxFrameBuffer;
					isTerminateMsg |= rxFrameBuffer == terminateKey;
					if(++rxWriteIndex == rxBufferSize)
					{
						rxWriteIndex = 0;
					}
					++rxCounter;
				}
			}
			break;
		}
	}

	void TxProcessing()
	{
		if(--timerTxCtr == 0)
		{
			timerTxCtr = 3;
			if(txCounter && txEnable)
			{
				if(txFrameCounter == txFrameSize)
				{
					txFrameBuffer = (((u16)txBuffer[txReadIndex])<<1) | 0x300; //Добавляем старт и стоп биты.
				}
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
					txFrameCounter = txFrameSize;
					if(++txReadIndex == txBufferSize)
					{
						txReadIndex = 0;
					}
					if(--txCounter == 0)
					{
						txEnable = false;
					}
					SetTxPinHigh();
				}
			}
		}
	}
	
	u8 Front()
	{
		return rxBuffer[rxReadIndex];
	}
	
	void Pop()
	{
		if(++rxReadIndex == rxBufferSize)
		{
			rxReadIndex = 0;
		}
		--rxCounter;
	}
	
	u8 FrontAndPop()
	{
		u8 data = Front();
		Pop();
		return data;
	}
	
	u8 Size()
	{
		return rxCounter;
	}
	
	bool NoEmpty()
	{
		return rxCounter > 0;
	}
	
	void Clear()
	{
		rxReadIndex = rxWriteIndex;
		rxCounter = 0;
	}
	
	bool IsTerminateMsg()
	{
		bool temp = isTerminateMsg;
		isTerminateMsg = false;
		return temp;
	}
	
	u8 Front(u8 index)
	{
		if(index >= rxCounter) return 0;
		index += rxReadIndex;
		if(index >= rxBufferSize) index -= rxBufferSize;
		return rxBuffer[index];
	}
	
	void Push(u8 data)
	{
		txBuffer[txWriteIndex] = data;
		if(++txWriteIndex == txBufferSize)
		{
			txWriteIndex = 0;
		}
		++txCounter;
	}
	
	void StartTransmit()
	{
		if(!txEnable)
		{
			txFrameCounter = txFrameSize;
			txEnable = true;
		}
	}
};

/*
 * SoftUART.cpp
 *
 * Created: 28.06.2017 7:36:17
 *  Author: Andrey
 */ 

#include "includefile.h"

#define SOFT_RX_PORT PIND
#define SOFT_TX_PORT PORTD
#define SOFT_RX_PIN 0
#define SOFT_TX_PIN 1

struct SoftUart
{	
	static const u8 rxBufferSize = 80;
	static const u8 txBufferSize = 80;
	inline u8 GetRxPin(){ return SOFT_RX_PORT & (1<<SOFT_RX_PIN); }
	inline void SetTxPinHigh(){SOFT_TX_PORT |= 1<<SOFT_RX_PIN;}
	inline void SetTxPinLow(){SOFT_TX_PORT &= ~(1<<SOFT_RX_PIN);}
	
	volatile u8 rxWriteIndex, rxReadIndex, rxCounter;
	volatile u8 rxBuffer[rxBufferSize];
	volatile u8 txWriteIndex, txReadIndex, txCounter, txEnable;
	volatile u8 txBuffer[txBufferSize];
	
	volatile enum
	{
		RxDisable,
		WaitStartBit,
		ReadData,
		WaitStopBit
	} rxFlag;
	u8 rxFrameBuffer, rxFrameCounter, timerRxCtr;
	static const u8 rxFrameDataBits = 8; //Количество бит данных
	
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
			}
			break;
			case ReadData: //Принимаем 8 бит
			if(--timerRxCtr == 0)
			{
				timerRxCtr = 3;
				rxFrameBuffer >>= 1;
				if(GetRxPin())
				{
					rxFrameBuffer |= 1<<(rxFrameDataBits-1);
				}
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
	
	u8 timerTxCtr, txFrameCounter = txFrameSize;
	u16 txFrameBuffer;
	static const u8 txFrameSize = 10; //Количество бит данных + стоп и старт бит
	
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
						txEnable = 0;
					}
					SetTxPinHigh();
				}
			}
		}
	}
};

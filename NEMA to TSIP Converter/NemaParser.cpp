/*
 * NemaParser.cpp
 *
 * Created: 29.06.2017 10:11:43
 *  Author: Andrey
 */ 

#include "stdafx.h"
#include "IUart.h"
#include <string.h>

class NemaParser
{
	public:

enum SentenceId {
	UNC,
	RMC,
	GGA
};

SentenceId ParseId(char sentenceBuf[])
{
	if (!strcmp(sentenceBuf, "RMC"))
	return RMC;
	if (!strcmp(sentenceBuf, "GGA"))
	return GGA;
	return UNC;
}

struct NmeaTime()
{
	 u8 hours;
	 u8 minutes;
	 u8 seconds;
	 u8 microseconds;
};

struct NmeaDate 
{
	u8 day;
	u8 month;
	u8 year;
};

	u8 ParserProcessing(IUart &uart)
	{
		if(uart.IsTerminateMsg())
		{
			//$GPGGA,hhmmss.ss,llll.lll,a,nnnnn.nnn,b,t,uu,v.v,w.w,M,x.x,M,y.y,zzzz*hh<CR><LF>
			while(uart.NoEmpty() && uart.FrontAndPop() != '$');
			if(uart.FrontAndPop() != 'G' || uart.FrontAndPop() != 'P') return 1;
			u8 checksum = 'G'^'P';
			char sentenceBuf[4];
			sentenceBuf[3] = 0;
			for(u8 i = 0; i < 3; i++)
			{
				char data = uart.FrontAndPop();
				if(data < 'A' || data > 'B') return 1;
				sentenceBuf[i] = data;
				checksum ^= data;
			}
			SentenceId id = ParseId(sentenceBuf);
			
			for(u8 i = 0; ;i++)
			{
				for(u8 j=0; ;j++)
				{
					
				}
			}
			switch(id)
			{
				case RMC:
				break;
				case GGA:
				break;
			}
			
		}
		return 0;
	}
	
	
	
	
	
};


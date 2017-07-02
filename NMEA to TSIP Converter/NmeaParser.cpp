/*
 * NmeaParser.cpp
 *
 * Created: 02.07.2017 20:51:05
 *  Author: Kelvin
 */ 


 class NmeaParser
 {
 #define MSG_ENCODE(_a,_b) ((u16(_a)<<8)|(_b))

 public:
void Parser(u8 data)
{
	static u8 byteCount;
	static u8 comaPoint;
	static u8 charPoint;
	static union
	{
		u16 msg16;
		u8 msg8[2];
	} msgType;
	
	switch(++byteCount)
	{
	case 1: if(data!='$') byteCount=0; break;
	case 2: if(data!='G') byteCount=0; break;
	case 3: if(data!='P' && data!='N') byteCount=0; break;
	case 4: if(data!='R' && data!='G') byteCount=0; break;
	case 5: msgType.msg8[1] = data; break;
	case 6: msgType.msg8[0] = data; break;
	default:
	if(data=='*') byteCount = 100;
	switch(msgType.msg16)
	{
	case MSG_ENCODE('',''):
	}
	}

	if(data=='$'){ByteCount=0;ComaPoint=0xff;MsgTxt=(char*)&MsgType; return;} //ждем начала стрки
	if(ByteCount==0xff) return;                                                                     //
	ByteCount++;
	if(ByteCount<=1)        return;                                                         //
	if(ByteCount<6&&ByteCount>1)            //берем 4 символа заголовка
	{
		*MsgTxt=data;   //и делаем из него число
		MsgTxt++;
		return;
	}
	//
	switch(MsgType)
	{
		case    0x434D5250:                             //GPRMC
		case    0x434D524E:                             //GNRMC
		if(data==',') {ComaPoint++;     CharPoint=0;RMC[ComaPoint][0]=0;return;}
		if(data=='*') {MsgType=0;return;}
		RMC[ComaPoint][CharPoint++]=data;
		RMC[ComaPoint][CharPoint]=0;
		return;
		case    0x41474750:                             //PGGA
		case    0x4147474e:                             //NGGA
		if(data==',')  {ComaPoint++;    CharPoint=0;GGA[ComaPoint][0]=0;return;}
		if(data=='*') {MsgType=0;return;}
		GGA[ComaPoint][CharPoint++]=data;
		GGA[ComaPoint][CharPoint]=0;
		return;
		case    0x47545650:             //PVTG
		if(data==',')  {ComaPoint++;    CharPoint=0;VTG[ComaPoint][0]=0;return;}
		if(data=='*') {return;}
		VTG[ComaPoint][CharPoint++]=data;
		VTG[ComaPoint][CharPoint]=0;
		return;
		case    0x4754564e:             //NVTG
		if(data==',')  {ComaPoint++;    CharPoint=0;VTG[ComaPoint][0]=0;return;}
		if(data=='*') {return;}
		VTG[ComaPoint][CharPoint++]=data;
		VTG[ComaPoint][CharPoint]=0;
		return;
		case    0x56534750:             //PGSV
		if(data==',')  {ComaPoint++;    CharPoint=0;GSV[ComaPoint][0]=0;return;}
		if(data=='*')  {GPS_COUNT=AsciiToInt(ViewSat);MsgType=0;return;}
		GSV[ComaPoint][CharPoint++]=data;
		GSV[ComaPoint][CharPoint]=0;
		return;
		case    0x5653474c:             //LGSV
		if(data==',')  {ComaPoint++;    CharPoint=0;GSV[ComaPoint][0]=0;return;}
		if(data=='*') {GLONAS_COUNT=AsciiToInt(ViewSat);MsgType=0;return;}
		GSV[ComaPoint][CharPoint++]=data;
		GSV[ComaPoint][CharPoint]=0;
		return;
		default:        ByteCount=0xff;break;
	}
	ByteCount=0xff;
}

 };
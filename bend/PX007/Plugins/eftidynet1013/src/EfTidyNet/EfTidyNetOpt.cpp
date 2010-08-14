#include "StdAfx.h"
#include <atlstr.h>
#include <vcclr.h>
#include "EfTidyNet.h"
using namespace System::Runtime::InteropServices;
using namespace EfTidyNet;
using namespace EfTidyNet::EfTidyOpt;




EfTidyOpt::TidyNetOpt::TidyNetOpt(TidyDoc % objTDoc)
{
	m_tdoc = objTDoc;
	m_vecEmptyTags		= gcnew List<String^>();
	m_vecInlineTags		= gcnew List<String^>();;
	m_vecPreTags		= gcnew List<String^>();;
	m_vecBlockTags		= gcnew List<String^>();;

}

bool EfTidyOpt::TidyNetOpt::LoadConfigFile(String^ SConfigFile)
{
	m_SconfigName = SConfigFile;
	CStringA strConfigFile(SConfigFile);
	return (tidyLoadConfig(m_tdoc,(ctmbstr)strConfigFile)== yes)?true:false;
}

bool EfTidyOpt::TidyNetOpt::ResetToDefaultValue()
{
	return (tidyOptResetAllToDefault(m_tdoc)== yes)?true:false;
}

//MarkUp
String^ EfTidyOpt::TidyNetOpt::Doctype()
{
	ctmbstr string;
	string=tidyOptGetValue(m_tdoc,TidyDoctype);
	return gcnew String((char*)string);
}
bool EfTidyOpt::TidyNetOpt::Doctype(String^ newVal)
{
	CStringA newStrVal(newVal);
	newStrVal= "\"" + newStrVal + "\"";
	return (tidyOptSetValue(m_tdoc, TidyDoctype,(ctmbstr)newStrVal)== yes)?true:false;

}
//property TidyMark: Add meta element indicating tidied doc
bool EfTidyOpt::TidyNetOpt::TidyMark()
{
	return  (tidyOptGetBool(m_tdoc,TidyOptionId::TidyMark)== yes) ? true : false;
}
bool EfTidyOpt::TidyNetOpt::TidyMark(bool newVal)
{
	Bool bVal = yes;
	if(!newVal)
		bVal = no;

	return (tidyOptSetBool( m_tdoc,TidyOptionId::TidyMark,bVal)== yes)? true : false;
}

bool EfTidyOpt::TidyNetOpt::HideEndTag()
{
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyHideEndTags)== yes) ? true : false;
}

bool EfTidyOpt::TidyNetOpt::HideEndTag(bool newVal)
{
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyHideEndTags,bVal)== yes)? true : false;
}

bool EfTidyOpt::TidyNetOpt::EncloseText()
{
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyEncloseBodyText)== yes) ? true : false;
}

bool EfTidyOpt::TidyNetOpt::EncloseText(bool newVal)
{
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool( m_tdoc,TidyOptionId::TidyEncloseBodyText,bVal)== yes)? true : false;
}

bool EfTidyOpt::TidyNetOpt::EncloseBlockText()
{
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyEncloseBlockText)== yes) ? true : false;
}

bool EfTidyOpt::TidyNetOpt::EncloseBlockText(bool newVal)
{
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyLogicalEmphasis,bVal)== yes)? true : false;; 
}

bool EfTidyOpt::TidyNetOpt::LogicalEmphasis()
{
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyLogicalEmphasis) == yes) ? true : false;
}

bool EfTidyOpt::TidyNetOpt::LogicalEmphasis(bool newVal)
{
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyEncloseBlockText,bVal)== yes)? true : false;; 

}

bool EfTidyOpt::TidyNetOpt::DefaultAltText()
{
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyAltText) == yes) ? true : false;
}
bool EfTidyOpt::TidyNetOpt::DefaultAltText(bool newVal)
{
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyAltText,bVal)== yes)? true : false; 
}


bool EfTidyOpt::TidyNetOpt::Clean()
{
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyMakeClean) == yes) ? true : false;
}
bool EfTidyOpt::TidyNetOpt::Clean(bool newVal)
{
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyMakeClean,bVal)== yes)? true : false; 
}


bool EfTidyOpt::TidyNetOpt::DropFontTags()
{
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyDropFontTags) == yes) ? true : false;
}

bool EfTidyOpt::TidyNetOpt::DropFontTags(bool newVal)
{
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyDropFontTags,bVal)== yes)? true : false; 
}

bool EfTidyOpt::TidyNetOpt::DropEmptyParas()
{
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyDropEmptyParas) == yes) ? true : false;
}
bool EfTidyOpt::TidyNetOpt::DropEmptyParas(bool newVal)
{
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyDropEmptyParas,bVal)== yes)? true : false; 
}
bool EfTidyOpt::TidyNetOpt::Word2000()
{
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyWord2000) == yes) ? true : false;
}
bool EfTidyOpt::TidyNetOpt::Word2000(bool newVal)
{
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyWord2000,bVal)== yes)? true : false; 
}


bool EfTidyOpt::TidyNetOpt::FixBadComment()
{
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyFixComments) == yes) ? true : false;
}
bool EfTidyOpt::TidyNetOpt::FixBadComment(bool newVal)
{
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyFixComments,bVal)== yes)? true : false; 
}

bool EfTidyOpt::TidyNetOpt::FixBackslash()
{
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyFixBackslash) == yes) ? true : false;
}
bool EfTidyOpt::TidyNetOpt::FixBackslash(bool newVal)
{
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyFixBackslash,bVal)== yes)? true : false; 
}

bool EfTidyOpt::TidyNetOpt::Bare()
{
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyMakeBare) == yes) ? true : false;
}

bool EfTidyOpt::TidyNetOpt::Bare(bool newVal)
{
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyMakeBare,bVal)== yes)? true : false; 
}

bool EfTidyOpt::TidyNetOpt::DropPropAttr()
{
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyDropPropAttrs) == yes) ? true : false;
}

bool EfTidyOpt::TidyNetOpt::DropPropAttr(bool newVal)
{
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyDropPropAttrs,bVal)== yes)? true : false; 
}

/*********************************************************************
Methods :-
1. ReturnTagsString
2. AddNewTags
3. ParseTags

Last Modified: 14Feb2008

Function Descrption: Private function for keeping track of total number of Tags
Interface Support = NO
*********************************************************************/

void EfTidyOpt::TidyNetOpt::AddNewTags(ETagsType Tags_Type, String^  a_csCommasTag)
{

	Bool bRet = no;
	CStringA csTags(a_csCommasTag);

	switch(Tags_Type)
	{
	case ETagsType::EMPTYTAGS: 
		bRet = tidyOptSetValue(m_tdoc,TidyEmptyTags,(ctmbstr)csTags);
		break;

	case ETagsType::INLINETAGS:
		bRet = tidyOptSetValue(m_tdoc,TidyInlineTags,(ctmbstr)csTags);
		break;

	case ETagsType::PRETAGS:
		bRet = tidyOptSetValue(m_tdoc,TidyPreTags,(ctmbstr)csTags);
		break;

	case ETagsType::BLOCKTAGS:
		bRet = tidyOptSetValue(m_tdoc,TidyBlockTags,(ctmbstr)csTags);
		break;
	}


}

void EfTidyOpt::TidyNetOpt::ParseTags(ETagsType Tags_Type,  String^  a_csTag)
{
	array<Char>^		charsDelim = {','};
	array<String^>^		arrTags = a_csTag->Split(charsDelim);


	for(unsigned int iCount =0; 
		iCount < arrTags->Length ;
		iCount++)
	{
		switch(Tags_Type)
		{
		case ETagsType::EMPTYTAGS: 
			m_vecEmptyTags->Add(arrTags[iCount]); 
			break;
		case ETagsType::INLINETAGS:
			m_vecInlineTags->Add(arrTags[iCount]); 
			break;
		case ETagsType::PRETAGS:
			m_vecPreTags->Add(arrTags[iCount]); 
			break;
		case ETagsType::BLOCKTAGS:
			m_vecBlockTags->Add(arrTags[iCount]); 
			break;
		}

		//add new Tags
		this->AddNewTags(Tags_Type,ReturnTagsString(Tags_Type));

	}


}
String^ EfTidyOpt::TidyNetOpt::ReturnTagsString(ETagsType tags_Type)
{
	String^ TagString = "";

	if(tags_Type == ETagsType::EMPTYTAGS)
	{
		for each(String^ dinosaur in m_vecEmptyTags )
		{
			TagString += dinosaur;
			TagString += ",";
		}
		//myEnumVect= m_vecEmptyTags.GetEnumerator();
	}
	else
		if(tags_Type == ETagsType::INLINETAGS)
		{
			for each(String^ dinosaur in m_vecInlineTags )
			{
				TagString += dinosaur;
				TagString += ",";
			}
		}
		else
			if(tags_Type == ETagsType::PRETAGS)
			{
				for each(String^ dinosaur in m_vecPreTags )
				{
					TagString += dinosaur;
					TagString += ",";
				}
			}
			else
				if(tags_Type == ETagsType::BLOCKTAGS)
				{
					for each(String^ dinosaur in m_vecBlockTags )
					{
						TagString += dinosaur;
						TagString += ",";
					}
				}

				TagString = TagString->Substring(0,TagString->Length -1);
				return TagString;
}

/*********************************************************************
Methods :-
1. NewEmptyTags

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/
String^ EfTidyOpt::TidyNetOpt::NewEmptyTags()
{
	return ReturnTagsString(ETagsType::EMPTYTAGS);
}
bool    EfTidyOpt::TidyNetOpt::NewEmptyTags(String ^ newVal)
{
	ParseTags(ETagsType::EMPTYTAGS,newVal);
	return true;
}
/*********************************************************************
Methods :-
1. NewInlineTags

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/
String^ EfTidyOpt::TidyNetOpt::NewInlineTags()
{
	return ReturnTagsString(ETagsType::INLINETAGS);
}

bool    EfTidyOpt::TidyNetOpt::NewInlineTags(String ^ newVal)
{
	ParseTags(ETagsType::INLINETAGS,newVal);
	return true;
}
/*********************************************************************
Methods :-
1. NewBlockLevelTags

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/
String^ EfTidyOpt::TidyNetOpt::NewBlockLevelTags()
{
	return ReturnTagsString(ETagsType::BLOCKTAGS);
}
bool    EfTidyOpt::TidyNetOpt::NewBlockLevelTags(String^ newVal)
{
	ParseTags(ETagsType::BLOCKTAGS,newVal);
	return true;
}
/*********************************************************************
Methods :-
1. NewPreTags

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/
String^ EfTidyOpt::TidyNetOpt::NewPreTags()
{
	return ReturnTagsString(ETagsType::PRETAGS);
}
bool    EfTidyOpt::TidyNetOpt::NewPreTags(String^ newVal)
{
	ParseTags(ETagsType::PRETAGS,newVal);
	return true;
}


/*********************************************************************
Methods :-
1. OutputType

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/
EOutputType  EfTidyOpt::TidyNetOpt::OutputType()
{

	if(tidyOptGetBool(m_tdoc, TidyXmlOut))
	{
		return EOutputType::XmlOut;
	}

	if(tidyOptGetBool(m_tdoc,TidyXhtmlOut))
	{
		return EOutputType::XhtmlOut;
	}

	if(tidyOptGetBool(m_tdoc,TidyHtmlOut))
	{
		return EOutputType::HtmlOut;
	}
	return EOutputType::HtmlOut;
}

bool EfTidyOpt::TidyNetOpt::OutputType(EOutputType newVal)
{

	if(newVal == EOutputType::XmlOut)
		return (tidyOptSetBool(m_tdoc, TidyXmlOut, yes ) == yes) ? true : false;

	if(newVal == EOutputType::XhtmlOut)
		return (tidyOptSetBool(m_tdoc, TidyXhtmlOut, yes )== yes) ? true : false;;

	if(newVal == EOutputType::HtmlOut)
		return (tidyOptSetBool(m_tdoc, TidyHtmlOut, yes )== yes) ? true : false;


	return false;
}



/*********************************************************************
Methods :-
1. InputAsXML

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/	
bool TidyNetOpt::InputAsXML()
{
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyXmlTags) == yes) ? true : false;
}

bool TidyNetOpt::InputAsXML(bool newVal)
{
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyXmlTags,bVal)== yes)? true : false; 
}
/*********************************************************************
Methods :-
1. ADDXmlDecl

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/	
bool TidyNetOpt::ADDXmlDecl()
{
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyXmlDecl) == yes) ? true : false;
}

bool TidyNetOpt::ADDXmlDecl(bool newVal)
{
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyXmlDecl,bVal)== yes)? true : false; 
}
/*********************************************************************
Methods :-
1. AddXmlSpace

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/	
bool TidyNetOpt::AddXmlSpace(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyXmlSpace) == yes) ? true : false;
}
bool TidyNetOpt::AddXmlSpace(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyXmlSpace,bVal)== yes)? true : false; 
}
/*********************************************************************
Methods :-
1. AssumeXmlProcins

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/	
bool TidyNetOpt::AssumeXmlProcins(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyXmlPIs) == yes) ? true : false;
}
bool TidyNetOpt::AssumeXmlProcins(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyXmlPIs,bVal)== yes)? true : false; 
}
/*********************************************************************
Methods :-
1. NumericsEntities

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/	

bool TidyNetOpt::NumericsEntities(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyNumEntities) == yes) ? true : false;
}
bool TidyNetOpt::NumericsEntities(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyNumEntities,bVal)== yes)? true : false; 
}
/*********************************************************************
Methods :-
1. QuoteMarks

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/	
bool TidyNetOpt::QuoteMarks(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyQuoteMarks) == yes) ? true : false;
}
bool TidyNetOpt::QuoteMarks(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyQuoteMarks,bVal)== yes)? true : false; 
}
/*********************************************************************
Methods :-
1. QuoteNBSP

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/	
bool TidyNetOpt::QuoteNBSP(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyQuoteNbsp) == yes) ? true : false;
}
bool TidyNetOpt::QuoteNBSP(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyQuoteNbsp,bVal)== yes)? true : false; 
}
/*********************************************************************
Methods :-
1. QuoteAmpersand

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/	
bool TidyNetOpt::QuoteAmpersand(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyQuoteAmpersand) == yes) ? true : false;
}
bool TidyNetOpt::QuoteAmpersand(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyQuoteAmpersand,bVal)== yes)? true : false; 
}

/*********************************************************************
Methods :-
1. CharEncoding

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/			
ECharEncodingType TidyNetOpt::CharEncoding()
{
	ctmbstr CharString;
	CharString=tidyOptGetEncName(m_tdoc,TidyCharEncoding);

	String^ strEncoded =gcnew String((char*)CharString);

	strEncoded->ToUpper();


	if(String::Compare(strEncoded,"ASCII")==0)
	{
		return ECharEncodingType::ASCII;		
	}


	if(String::Compare(strEncoded,"LATIN1")==0)
	{
		return ECharEncodingType::LATIN1;		
	}

	if(String::Compare(strEncoded,"RAW")==0)
	{
		return ECharEncodingType::RAW;		
	}


	if(String::Compare(strEncoded,"UTF8")==0)
	{
		return ECharEncodingType::UTF8;		
	}

	if(String::Compare(strEncoded,"ISO2022")==0)
	{
		return ECharEncodingType::ISO2022;		
	}

	if(String::Compare(strEncoded,"MAC")==0)
	{
		return ECharEncodingType::MAC;		
	}

	if(String::Compare(strEncoded,"WIN1252")==0)
	{
		return ECharEncodingType::WIN1252;		
	}

	if(String::Compare(strEncoded,"UTF16LE")==0)
	{
		return ECharEncodingType::UTF16LE;		
	}

	if(String::Compare(strEncoded,"UTF16BE")==0)
	{
		return ECharEncodingType::UTF16BE;		
	}
	if(String::Compare(strEncoded,"UTF16")==0)
	{
		return ECharEncodingType::UTF16BE;		
	}


	if(String::Compare(strEncoded,"BIG5")==0)
	{
		return ECharEncodingType::BIG5;		
	}

	if(String::Compare(strEncoded,"SHIFTJIS")==0)
	{
		return ECharEncodingType::SHIFTJIS;		
	}

	return ECharEncodingType::CHARUNKNOWN;
}

bool TidyNetOpt::CharEncoding(ECharEncodingType newVal)
{
	//Setting new character Encoding
	CStringA CharEncode;

	switch(newVal)
	{
	case ECharEncodingType::ASCII		:	CharEncode="ASCII"; break;
	case ECharEncodingType::LATIN1		:	CharEncode="LATIN1"; break;
	case ECharEncodingType::RAW			:	CharEncode="RAW"; break;
	case ECharEncodingType::UTF8		:	CharEncode="UTF8"; break;
	case ECharEncodingType::ISO2022		:	CharEncode="ISO2022"; break;
	case ECharEncodingType::MAC			:	CharEncode="MAC"; break;
	case ECharEncodingType::WIN1252		:	CharEncode="WIN1252"; break;
	case ECharEncodingType::UTF16LE		:	CharEncode="UTF16LE"; break;
	case ECharEncodingType::UTF16BE		:	CharEncode="UTF16BE"; break;
	case ECharEncodingType::UTF16		:	CharEncode="UTF16"; break;
	case ECharEncodingType::BIG5		:	CharEncode="BIG5"; break;
	case ECharEncodingType::SHIFTJIS	:	CharEncode="SHIFTJIS"; break;

	}
	return ( tidySetCharEncoding(m_tdoc,(ctmbstr)CharEncode) == yes) ? true :false;
}


/*********************************************************************
Methods :-
1. InCharEncoding

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/	
ECharEncodingType TidyNetOpt::InCharEncoding()
{
	ctmbstr CharString;
	CharString=tidyOptGetEncName(m_tdoc,TidyInCharEncoding);

	String^ strEncoded =gcnew String((char*)CharString);

	strEncoded->ToUpper();


	if(String::Compare(strEncoded,"ASCII")==0)
	{
		return ECharEncodingType::ASCII;		
	}


	if(String::Compare(strEncoded,"LATIN1")==0)
	{
		return ECharEncodingType::LATIN1;		
	}

	if(String::Compare(strEncoded,"RAW")==0)
	{
		return ECharEncodingType::RAW;		
	}


	if(String::Compare(strEncoded,"UTF8")==0)
	{
		return ECharEncodingType::UTF8;		
	}

	if(String::Compare(strEncoded,"ISO2022")==0)
	{
		return ECharEncodingType::ISO2022;		
	}

	if(String::Compare(strEncoded,"MAC")==0)
	{
		return ECharEncodingType::MAC;		
	}

	if(String::Compare(strEncoded,"WIN1252")==0)
	{
		return ECharEncodingType::WIN1252;		
	}

	if(String::Compare(strEncoded,"UTF16LE")==0)
	{
		return ECharEncodingType::UTF16LE;		
	}

	if(String::Compare(strEncoded,"UTF16BE")==0)
	{
		return ECharEncodingType::UTF16BE;		
	}
	if(String::Compare(strEncoded,"UTF16")==0)
	{
		return ECharEncodingType::UTF16BE;		
	}


	if(String::Compare(strEncoded,"BIG5")==0)
	{
		return ECharEncodingType::BIG5;		
	}

	if(String::Compare(strEncoded,"SHIFTJIS")==0)
	{
		return ECharEncodingType::SHIFTJIS;		
	}

	return ECharEncodingType::CHARUNKNOWN;
}

bool TidyNetOpt::InCharEncoding(ECharEncodingType newVal)
{
	//Setting new character Encoding
	CStringA CharEncode;

	switch(newVal)
	{
	case ECharEncodingType::ASCII		:	CharEncode="ASCII"; break;
	case ECharEncodingType::LATIN1		:	CharEncode="LATIN1"; break;
	case ECharEncodingType::RAW		:	CharEncode="RAW"; break;
	case ECharEncodingType::UTF8		:	CharEncode="UTF8"; break;
	case ECharEncodingType::ISO2022	:	CharEncode="ISO2022"; break;
	case ECharEncodingType::MAC		:	CharEncode="MAC"; break;
	case ECharEncodingType::WIN1252	:	CharEncode="WIN1252"; break;
	case ECharEncodingType::UTF16LE	:	CharEncode="UTF16LE"; break;
	case ECharEncodingType::UTF16BE	:	CharEncode="UTF16BE"; break;
	case ECharEncodingType::UTF16		:	CharEncode="UTF16"; break;
	case ECharEncodingType::BIG5		:	CharEncode="BIG5"; break;
	case ECharEncodingType::SHIFTJIS	:	CharEncode="SHIFTJIS"; break;

	}
	return ( tidySetInCharEncoding(m_tdoc,(ctmbstr)CharEncode) == yes) ? true :false;
}

/*********************************************************************
Methods :-
1. OutCharEncoding

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/	
ECharEncodingType TidyNetOpt::OutCharEncoding()
{
	ctmbstr CharString;
	CharString=tidyOptGetEncName(m_tdoc,TidyOutCharEncoding);

	String^ strEncoded =gcnew String((char*)CharString);

	strEncoded->ToUpper();


	if(String::Compare(strEncoded,"ASCII")==0)
	{
		return ECharEncodingType::ASCII;		
	}


	if(String::Compare(strEncoded,"LATIN1")==0)
	{
		return ECharEncodingType::LATIN1;		
	}

	if(String::Compare(strEncoded,"RAW")==0)
	{
		return ECharEncodingType::RAW;		
	}


	if(String::Compare(strEncoded,"UTF8")==0)
	{
		return ECharEncodingType::UTF8;		
	}

	if(String::Compare(strEncoded,"ISO2022")==0)
	{
		return ECharEncodingType::ISO2022;		
	}

	if(String::Compare(strEncoded,"MAC")==0)
	{
		return ECharEncodingType::MAC;		
	}

	if(String::Compare(strEncoded,"WIN1252")==0)
	{
		return ECharEncodingType::WIN1252;		
	}

	if(String::Compare(strEncoded,"UTF16LE")==0)
	{
		return ECharEncodingType::UTF16LE;		
	}

	if(String::Compare(strEncoded,"UTF16BE")==0)
	{
		return ECharEncodingType::UTF16BE;		
	}
	if(String::Compare(strEncoded,"UTF16")==0)
	{
		return ECharEncodingType::UTF16BE;		
	}


	if(String::Compare(strEncoded,"BIG5")==0)
	{
		return ECharEncodingType::BIG5;		
	}

	if(String::Compare(strEncoded,"SHIFTJIS")==0)
	{
		return ECharEncodingType::SHIFTJIS;		
	}

	return ECharEncodingType::CHARUNKNOWN;
}

bool TidyNetOpt::OutCharEncoding(ECharEncodingType newVal){
	//Setting new character Encoding
	CStringA CharEncode;

	switch(newVal)
	{
	case ECharEncodingType::ASCII		:	CharEncode="ASCII"; break;
	case ECharEncodingType::LATIN1		:	CharEncode="LATIN1"; break;
	case ECharEncodingType::RAW		:	CharEncode="RAW"; break;
	case ECharEncodingType::UTF8		:	CharEncode="UTF8"; break;
	case ECharEncodingType::ISO2022	:	CharEncode="ISO2022"; break;
	case ECharEncodingType::MAC		:	CharEncode="MAC"; break;
	case ECharEncodingType::WIN1252	:	CharEncode="WIN1252"; break;
	case ECharEncodingType::UTF16LE	:	CharEncode="UTF16LE"; break;
	case ECharEncodingType::UTF16BE	:	CharEncode="UTF16BE"; break;
	case ECharEncodingType::UTF16		:	CharEncode="UTF16"; break;
	case ECharEncodingType::BIG5		:	CharEncode="BIG5"; break;
	case ECharEncodingType::SHIFTJIS	:	CharEncode="SHIFTJIS"; break;

	}
	return ( tidySetOutCharEncoding(m_tdoc,(ctmbstr)CharEncode) == yes) ? true :false;
}

/*********************************************************************
Methods :-
1. OutputTagInUpperCase

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/	

bool TidyNetOpt::OutputTagInUpperCase(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyUpperCaseTags) == yes) ? true : false;
};
bool TidyNetOpt::OutputTagInUpperCase(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyUpperCaseTags,bVal)== yes)? true : false; 
}
/*********************************************************************
Methods :-
1. OutputTagInUpperCase

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/	
bool TidyNetOpt::OutputAttrInUpperCase(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyUpperCaseAttrs) == yes) ? true : false;
}
bool TidyNetOpt::OutputAttrInUpperCase(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyUpperCaseAttrs,bVal)== yes)? true : false; 
}
/*********************************************************************
Methods :-
1. OutputTagInUpperCase

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/	
bool TidyNetOpt::WrapScriptlets(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyWrapScriptlets) == yes) ? true : false;
}
bool TidyNetOpt::WrapScriptlets(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyWrapScriptlets,bVal)== yes)? true : false; 
}
/*********************************************************************
Methods :-
1. OutputTagInUpperCase

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/	
bool TidyNetOpt::WrapAttVals(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyWrapAttVals) == yes) ? true : false;
}
bool TidyNetOpt::WrapAttVals(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyWrapAttVals,bVal)== yes)? true : false; 
}
/*********************************************************************
Methods :-
1. OutputTagInUpperCase

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/	
bool TidyNetOpt::WrapSection(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyWrapSection) == yes) ? true : false;
}
bool TidyNetOpt::WrapSection(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyWrapSection,bVal)== yes)? true : false; 
}
/*********************************************************************
Methods :-
1. OutputTagInUpperCase

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/	
bool TidyNetOpt::WrapAsp(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyWrapAsp) == yes) ? true : false;
}
bool TidyNetOpt::WrapAsp(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyWrapAsp,bVal)== yes)? true : false; 
}
/*********************************************************************
Methods :-
1. OutputTagInUpperCase

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/	
bool TidyNetOpt::WrapJste(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyWrapJste) == yes) ? true : false;
}
bool TidyNetOpt::WrapJste(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyWrapJste,bVal)== yes)? true : false; 
}
/*********************************************************************
Methods :-
1. OutputTagInUpperCase

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/	
bool TidyNetOpt::WrapPhp(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyWrapPhp) == yes) ? true : false;
}
bool TidyNetOpt::WrapPhp(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyWrapPhp,bVal)== yes)? true : false; 
}
/*********************************************************************
Methods :-
1. OutputTagInUpperCase

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/	
bool TidyNetOpt::IndentAttributes(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyIndentAttributes) == yes) ? true : false;
}
bool TidyNetOpt::IndentAttributes(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyIndentAttributes,bVal)== yes)? true : false; 
}
/*********************************************************************
Methods :-
1. OutputTagInUpperCase

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/			
bool TidyNetOpt::BreakBeforeBR(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyBreakBeforeBR) == yes) ? true : false;
}
bool TidyNetOpt::BreakBeforeBR(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyBreakBeforeBR,bVal)== yes)? true : false; 
}
/*********************************************************************
Methods :-
1. OutputTagInUpperCase

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/	
bool TidyNetOpt::LiteralAttribs(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyLiteralAttribs) == yes) ? true : false;
}
bool TidyNetOpt::LiteralAttribs(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyLiteralAttribs,bVal)== yes)? true : false; 
}
/*********************************************************************
Methods :-
1. OutputTagInUpperCase

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/			
long TidyNetOpt::IndentSpace(){
	return tidyOptGetInt(m_tdoc, TidyIndentSpaces);
}
bool TidyNetOpt::IndentSpace(long newVal){

	return (tidyOptSetInt(m_tdoc, TidyOptionId::TidyIndentSpaces,newVal)== yes)? true : false; 
}
/*********************************************************************
Methods :-
1. OutputTagInUpperCase

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/	
long TidyNetOpt::WrapLen(){
	return tidyOptGetInt(m_tdoc, TidyOptionId::TidyWrapLen);
}
bool TidyNetOpt::WrapLen(long newVal){
	return (tidyOptSetInt(m_tdoc, TidyOptionId::TidyWrapLen,newVal)== yes)? true : false; 
}
/*********************************************************************
Methods :-
1. OutputTagInUpperCase

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/	
long TidyNetOpt::TabSize(){
	return tidyOptGetInt(m_tdoc, TidyOptionId::TidyTabSize);
}
bool TidyNetOpt::TabSize(long newVal){
	return (tidyOptSetInt(m_tdoc, TidyOptionId::TidyTabSize,newVal)== yes)? true : false;
}
/*********************************************************************
Methods :-
1. OutputTagInUpperCase

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/		
EIndentScheme TidyNetOpt::Indent()
{

	switch(tidyOptGetInt( m_tdoc, TidyIndentContent))
	{
	case 0:
		return EIndentScheme::NOINDENT;

	case 1:
		return EIndentScheme::INDENTBLOCKS;

	case 2:
		return EIndentScheme::AUTOINDENT;

	}
return EIndentScheme::NOINDENT;

}
bool TidyNetOpt::Indent(EIndentScheme newVal){
	switch(newVal)
	{
	case EIndentScheme::NOINDENT:
		return (tidyOptSetInt( m_tdoc, TidyIndentContent,TidyTriState::TidyNoState)== yes)?true:false;
		break;
	case EIndentScheme::INDENTBLOCKS:
		return (tidyOptSetInt( m_tdoc, TidyIndentContent,TidyTriState::TidyYesState)== yes)?true:false;
		break;
	case EIndentScheme::AUTOINDENT:
		return (tidyOptSetInt(m_tdoc, TidyIndentContent,TidyTriState::TidyAutoState)== yes)?true:false;
		break;
	}
	return (tidyOptSetInt(m_tdoc, TidyIndentContent,TidyTriState::TidyAutoState)== yes)?true:false;

}
/*********************************************************************
Methods :-
1. OutputTagInUpperCase

Last Modified: 14Feb2008

Function Descrption: NewEmptytags
Interface Support = NO
*********************************************************************/	

//Operation
bool TidyNetOpt::MarkUp(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyShowMarkup) == yes) ? true : false;
}
bool TidyNetOpt::MarkUp(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyShowMarkup,bVal)== yes)? true : false; 
}
bool TidyNetOpt::ShowWarnings(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyShowWarnings) == yes) ? true : false;
}
bool TidyNetOpt::ShowWarnings(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyShowWarnings,bVal)== yes)? true : false; 
}
bool TidyNetOpt::Quiet(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyQuiet) == yes) ? true : false;
}
bool TidyNetOpt::Quiet(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyQuiet,bVal)== yes)? true : false; 
}

bool TidyNetOpt::KeepTime(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyKeepFileTimes) == yes) ? true : false;
}
bool TidyNetOpt::KeepTime(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyKeepFileTimes,bVal)== yes)? true : false; 
}
bool TidyNetOpt::GnuEmacs(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyEmacs) == yes) ? true : false;
}
bool TidyNetOpt::GnuEmacs(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyEmacs,bVal)== yes)? true : false; 
}
//newly added
bool TidyNetOpt::FixUrl(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyFixUri) == yes) ? true : false;
}
bool TidyNetOpt::FixUrl(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyFixUri,bVal)== yes)? true : false; 
}

bool TidyNetOpt::BodyOnly(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyBodyOnly) == yes) ? true : false;
}
bool TidyNetOpt::BodyOnly(bool newVal){
	TidyTriState bVal = TidyTriState::TidyYesState;
	if(!newVal)
		bVal = TidyTriState::TidyNoState;

	return (tidyOptSetInt( m_tdoc, TidyOptionId::TidyBodyOnly,bVal)== yes)?true:false;
		//(tidyOptSetBool(m_tdoc,(TidyOptionId)59,bVal)== yes)? true : false; 
}
bool TidyNetOpt::HideComments(){
	return (tidyOptGetBool(m_tdoc,TidyOptionId::TidyHideComments) == yes) ? true : false;
}
bool TidyNetOpt::HideComments(bool newVal){
	Bool bVal = yes;
	if(!newVal)
		bVal = no;
	return (tidyOptSetBool(m_tdoc,TidyOptionId::TidyHideComments,bVal)== yes)? true : false; 
}
String^ TidyNetOpt::ErrorFile(){

	ctmbstr post;
	post = tidyOptGetValue(m_tdoc, TidyOptionId::TidyErrFile );
   
	return gcnew String((char*)post);
}

void TidyNetOpt::ErrorFile(String^ newVal)
{
	CStringA str(newVal);
	tidySetErrorFile( m_tdoc,(ctmbstr)str);
}


EDoctypeModes TidyNetOpt::DoctypeMode()
{
	return (EDoctypeModes)(int) tidyOptGetInt(m_tdoc,TidyDoctypeMode);
}
bool TidyNetOpt::DoctypeMode(EDoctypeModes newVal)
{
	return (tidyOptSetInt(m_tdoc,TidyDoctypeMode,(int)newVal)== yes)?true:false;
}

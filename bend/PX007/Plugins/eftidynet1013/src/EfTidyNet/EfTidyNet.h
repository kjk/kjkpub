// EfTidyNet.h
/**

Author : Alok Gupta
Email  : thatsalok@gmail.com

You are Free to Modify,use code etc,till above name is not removed.
I am not responisible for problem coming while using above code. as it provided AS IS without any warranty.
Please use it at your own Risk.

It is not compulsory to notify me if you using this component or changing the code, but it would be appreciated.
Thanks
**/
#pragma once
#include "..\\TidyMyLib\\tidy.h"
#include "..\\TidyMyLib\\buffio.h"
#pragma comment(lib,"..\\output\\tidymylib.lib")

using namespace System;
using namespace System::Collections::Generic;


namespace  EfTidyNet 
{
	namespace EfTidyOpt
	{
		
		public  enum class ECharEncodingType
		{
		 ASCII		= 0 ,
		 LATIN1		= 1 , 
		 RAW		= 2, 
		 UTF8		= 3, 
		 ISO2022	= 4,
		 MAC		= 5,
		 WIN1252	= 6,
		 UTF16LE	= 7,
		 UTF16BE	= 8,
		 UTF16		= 9,
		 BIG5		= 10,
		 SHIFTJIS	= 11,
		 CHARUNKNOWN	= 12
		} ;

		[Flags]
		public  enum class EOutputType {
		  XmlOut=1,          /**< Create output as XML */
		  XhtmlOut=2,        /**< Output extensible HTML */
		  HtmlOut=4         /**< Output plain HTML, even for XHTML input.*/
		};


		[Flags]
		public  enum class EIndentScheme 
		{
			NOINDENT=0,
			INDENTBLOCKS,
			AUTOINDENT
		} ;
	
		[Flags]
		public  enum class EDoctypeModes
		{
		    DoctypeOmit,    /**< Omit DOCTYPE altogether */
		    DoctypeAuto,    /**< Keep DOCTYPE in input.  Set version to content */
		    DoctypeStrict,  /**< Convert document to HTML 4 strict content model */
		    DoctypeLoose,   /**< Convert document to HTML 4 transitional
		                            content model */
		    DoctypeUser     /**< Set DOCTYPE FPI explicitly */
		} ;

		typedef enum tagTypes
		{
			EMPTYTAGS,INLINETAGS,PRETAGS,BLOCKTAGS
		} ETagsType;
		public ref class TidyNetOpt
		{
			TidyDoc			m_tdoc;
			List<String^>^  m_vecEmptyTags;
			List<String^>^  m_vecInlineTags;
			List<String^>^  m_vecPreTags;
			List<String^>^  m_vecBlockTags;
			String^			m_SconfigName;

			String^ ReturnTagsString(ETagsType  Tags_Type);
			void ParseTags		 (ETagsType  Tags_Type, String^  TagString);
			void AddNewTags		 (ETagsType  Tags_Type,	String^  TagString);

		public:
			
			TidyNetOpt(TidyDoc %);
			bool LoadConfigFile(String^ SConfigFile);
			bool ResetToDefaultValue();
			//MarkUp
			String^ Doctype();
			bool Doctype(String^ newVal);

			//property TidyMark: Add meta element indicating tidied doc
			bool TidyMark();
			bool TidyMark(bool newVal);

			bool HideEndTag();
			bool HideEndTag(bool newVal);

			bool EncloseText();
			bool EncloseText(bool newVal);

			bool EncloseBlockText();
			bool EncloseBlockText(bool newVal);

			bool LogicalEmphasis();
			bool LogicalEmphasis(bool newVal);

			bool DefaultAltText();
			bool DefaultAltText(bool  newVal);

			bool Clean();
			bool Clean(bool newVal);

			bool DropFontTags();
			bool DropFontTags(bool newVal);

			bool DropEmptyParas();
			bool DropEmptyParas(bool newVal);

			bool Word2000();
			bool Word2000(bool newVal);

			bool FixBadComment();
			bool FixBadComment(bool newVal);

			bool FixBackslash();
			bool FixBackslash(bool newVal);

			bool Bare();
			bool Bare(bool newVal);

			bool DropPropAttr();
			bool DropPropAttr(bool newVal);

			String^ NewEmptyTags();
			bool NewEmptyTags(String ^ newVal);

			String^ NewInlineTags();
			bool NewInlineTags(String ^ newVal);

			String^ NewBlockLevelTags();
			bool NewBlockLevelTags(String^ newVal);

			String^ NewPreTags();
			bool    NewPreTags(String^ newVal);

			//Outputtype
 			EOutputType  OutputType();
			bool OutputType(EOutputType newVal);
	
			bool InputAsXML();
			bool InputAsXML(bool newVal);
			
			bool ADDXmlDecl();
			bool ADDXmlDecl(bool newVal);
	
			bool AddXmlSpace();
			bool AddXmlSpace(bool newVal);

		    bool AssumeXmlProcins();
			bool AssumeXmlProcins(bool newVal);
	
			////EnCoding
			
			ECharEncodingType CharEncoding();
			bool CharEncoding(ECharEncodingType newVal);

			bool NumericsEntities();
		    bool NumericsEntities(bool newVal);

			bool QuoteMarks();
			bool QuoteMarks(bool newVal);
	
			bool QuoteNBSP();
			bool QuoteNBSP(bool newVal);

			bool QuoteAmpersand();
			bool QuoteAmpersand(bool newVal);
			
			//	
			//  //New Added in encoding
			ECharEncodingType InCharEncoding();
			bool InCharEncoding(ECharEncodingType newVal);
			ECharEncodingType OutCharEncoding();
			bool OutCharEncoding(ECharEncodingType newVal);

			//Layout
			bool OutputTagInUpperCase();
			bool OutputTagInUpperCase(bool newVal);

			bool OutputAttrInUpperCase();
			bool OutputAttrInUpperCase(bool newVal);

			bool WrapScriptlets();
			bool WrapScriptlets(bool newVal);

			bool WrapAttVals();
			bool WrapAttVals(bool newVal);

			bool WrapSection();
			bool WrapSection(bool newVal);

			bool WrapAsp();
			bool WrapAsp(bool newVal);

			bool WrapJste();
			bool WrapJste(bool newVal);

			bool WrapPhp();
			bool WrapPhp(bool newVal);
	
			bool IndentAttributes();
			bool IndentAttributes(bool newVal);
		
			bool BreakBeforeBR();
			bool BreakBeforeBR(bool newVal);

			bool LiteralAttribs();
			bool LiteralAttribs(bool newVal);

		
			long IndentSpace();
			bool IndentSpace(long newVal);

			long WrapLen();
			bool WrapLen(long newVal);

			long TabSize();
			bool TabSize(long newVal);
		
			EIndentScheme Indent();
			bool Indent(EIndentScheme newVal);

			//Operation
			bool MarkUp();
			bool MarkUp(bool newVal);

			bool ShowWarnings();
			bool ShowWarnings(bool newVal);

			bool Quiet();
			bool Quiet(bool newVal);

			bool KeepTime();
			bool KeepTime(bool newVal);

			bool GnuEmacs();
			bool GnuEmacs(bool newVal);
		
			
		
		//newly added
			bool FixUrl();
			bool FixUrl(bool newVal);

			bool BodyOnly();
			bool BodyOnly(bool newVal);

			bool HideComments();
			bool HideComments(bool newVal);

			String^ ErrorFile();
			void ErrorFile(String^ newVal);
		

			EDoctypeModes DoctypeMode();
			bool DoctypeMode(EDoctypeModes newVal);
	

		};
	}

	public ref class TidyNet : public IDisposable
	{
		bool m_bOperationPerformed;
		 TidyDoc m_tdoc;// = tidyCreate();
		//static EfTidyOpt::TidyNetOpt^	m_objOption = gcnew EfTidyOpt::TidyNetOpt(m_tdoc);
		String^			m_SError;
		bool			m_bDisposing;

	public:
		TidyNet()
		{
			 m_bDisposing = false;
			 m_bOperationPerformed = false;
			 
			 m_tdoc = tidyCreate();
			 Option = gcnew EfTidyOpt::TidyNetOpt(m_tdoc);
		}

		TidyNet(TidyNet % obj){}

		bool TidyFiletoMem (const String^ SFileName	 , String^ %		SResult);
		bool TidyFileToFile(String^ SsourceFileName  , String^			SDestFile);
		bool TidyMemToMem  (String^ SsourceData		 , String^ %		SResult);
		bool TidyMemtoFile (String^ SBuffer			 , String^			SDestFile);
		bool TotalWarnings (long %pVal);
		bool TotalErrors   (long %pVal);
		String^ ErrorWarning  ();		
		//EfTidyOpt::TidyNetOpt^ Option(){ return m_objOption;}
		EfTidyOpt::TidyNetOpt^	Option;
		
		void CloseDocument()
		{
			this ->!TidyNet();
			m_bDisposing = true;
		}


		~TidyNet()
		{
			if(!m_bDisposing)
			{
				this->!TidyNet();
				m_bDisposing = true;
			}
		}

		!TidyNet()
		{
			if(m_tdoc != NULL)
			{
				tidyRelease(m_tdoc);
				m_tdoc = NULL;
			}
		}

	};


}

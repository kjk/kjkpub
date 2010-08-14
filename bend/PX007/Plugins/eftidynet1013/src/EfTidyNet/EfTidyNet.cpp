// This is the main DLL file.

#include "stdafx.h"
#include <atlstr.h>
#include <vcclr.h>
#include "EfTidyNet.h"
using namespace System::Runtime::InteropServices;

bool EfTidyNet::TidyNet::TidyFiletoMem(const String^ SFileName, String^ % SResult)
{
	TidyBuffer output={0};
	TidyBuffer errbuf={0};
	bool bOK = true;						  // Convert to XHTML
	int rc=-1;

	CStringA strFileName(SFileName); //= (char*)(void*)Marshal::StringToHGlobalAnsi(SFileName);
		
		
	if ( bOK )
	{
	  rc = tidySetErrorBuffer(this->m_tdoc, &errbuf );      // Capture diagnostics
	}
	
	if ( rc >= 0 )
	{
	  rc = ::tidyParseFile(this->m_tdoc,(ctmbstr)strFileName);//tidyParseString( tdoc, input );           // Parse the input
	}

	if ( rc >= 0 )
	{
		rc = tidyCleanAndRepair(this->m_tdoc);               // Tidy it up!
	}
	
	if ( rc >= 0 )
	{
		rc = tidyRunDiagnostics(this->m_tdoc);               // Kvetch
	}
	
	if ( rc > 1 )                                    // If error, force output.
	{
		rc = ( tidyOptSetBool(this->m_tdoc, TidyForceOutput, yes) ? rc : -1 );
	}
	
	if ( rc >= 0 )
	{
		rc = tidySaveBuffer(this->m_tdoc, &output );          // Pretty Print
	}
      
	

	SResult  = gcnew String((char *)output.bp);
	m_SError = gcnew String((char*)errbuf.bp);
	
	//Free
	tidyBufFree( &output );
	tidyBufFree( &errbuf );
	strFileName.Empty();
		
	m_bOperationPerformed=true;
	
	return false;
}

// Change History!
// 27 FEB 08 - changes to incorporate new tidy code!
bool EfTidyNet::TidyNet::TidyFileToFile(String^ SsourceFileName   ,  String^ SDestFile)
{


	if(!System::IO::File::Exists(SsourceFileName))
	{
		m_SError = gcnew String("File Doesn\'t Exists");
		return false;
	}

	//some useful variable declaration
	CStringA strSrcFileName(SsourceFileName);
	CStringA strDestFileName(SDestFile);

	
	TidyBuffer errbuf={0};
	BOOL bOK;
	int rc=-1;
	
	bOK = TRUE;//tidyOptSetBool(this->m_tdoc, TidyXhtmlOut, yes );  // Convert to XHTML

	if ( bOK )
	{
		rc = tidySetErrorBuffer(this->m_tdoc, &errbuf );      // Capture diagnostics
	}
	
	if ( rc >= 0 )
	{
		rc = ::tidyParseFile(this->m_tdoc,(ctmbstr)strSrcFileName);//tidyParseString( tdoc, input );           // Parse the input
	}
	
	if ( rc >= 0 )
	{
		rc = tidyCleanAndRepair(this->m_tdoc);               // Tidy it up!
	}
	
	if ( rc >= 0 )
	{
		rc = tidyRunDiagnostics(this->m_tdoc);               // Kvetch
	}
	
	if ( rc > 1 )                                    // If error, force output.
	{
		rc = ( tidyOptSetBool(this->m_tdoc, TidyForceOutput, yes) ? rc : -1 );
	}
	
	if ( rc >= 0 )
	{
		rc = tidySaveFile(this->m_tdoc,(ctmbstr)strDestFileName );          // Pretty Print
	}
      
	//copy ther error Buffer
	m_SError=gcnew String((char*)errbuf.bp);

	tidyBufFree( &errbuf );
    m_bOperationPerformed=true;
	return true;
}
bool EfTidyNet::TidyNet::TidyMemToMem  (String^ SsourceData		, String^ %		SResult)
{
	if(SsourceData->Length == 0)
	{
		m_SError = gcnew String("Buffer is Empty i.e. First Parameter is Zero Length String");
		return false;
	}
	//some useful variable declaration
	CStringA strSourceData(SsourceData);
	
	
	TidyBuffer output={0};
	TidyBuffer errbuf={0};

	//Initialise it to ZERO
	tidyBufInit(&output);
	tidyBufInit(&errbuf);
	BOOL bOK;
	int rc=-1;

	
	bOK =TRUE; //tidyOptSetBool(this->m_tdoc, TidyXhtmlOut, yes );  // Convert to XHTML
	
	if ( bOK )
	{
	  rc = tidySetErrorBuffer(this->m_tdoc, &errbuf );      // Capture diagnostics
	}
	
	if ( rc >= 0 )
	{
		rc = tidyParseString(this->m_tdoc,(ctmbstr)strSourceData);//tidyParseString( tdoc, input );           // Parse the input
	}
	
	if ( rc >= 0 )
	{
		rc = tidyCleanAndRepair(this->m_tdoc);               // Tidy it up!
	}
	
	if ( rc >= 0 )
	{
		rc = tidyRunDiagnostics(this->m_tdoc);               // Kvetch
	}
	
	if ( rc > 1 )                                    // If error, force output.
	{
		rc = ( tidyOptSetBool(this->m_tdoc, TidyForceOutput, yes) ? rc : -1 );
	}
	
	if ( rc >= 0 )
	{
		rc = tidySaveBuffer(this->m_tdoc, &output );          // Pretty Print
	}
      
	SResult	=gcnew String((char *)output.bp);
	m_SError=gcnew String((char*)errbuf.bp);

	 //free
	strSourceData.Empty();
	tidyBufFree( &output );
	tidyBufFree( &errbuf );

	m_bOperationPerformed=true;
	return true;

}

bool EfTidyNet::TidyNet::TidyMemtoFile (String^ SBuffer			, String^ SDestFile)
{
	if(SBuffer->Length == 0)
	{
		m_SError = gcnew String("Buffer is Empty i.e. First Parameter is Zero Length String");
		return false;
	}
	
	if(SDestFile->Length==0)
	{
		m_SError = gcnew String("Destination fileName is Not Given");
		return false;
	}
	
	//some useful variable declaration
	CStringA strBuffer(SBuffer);
	CStringA strDestFileName(SDestFile);
	

	TidyBuffer errbuf={0};
	BOOL bOK;
	int rc=-1;

	
	bOK =TRUE; //tidyOptSetBool(this->m_tdoc, TidyXhtmlOut, yes );  // Convert to XHTML
	
	if ( bOK )
	{
		rc = tidySetErrorBuffer(m_tdoc, &errbuf );      // Capture diagnostics
	}
	
	if ( rc >= 0 )
	{
		rc = tidyParseString( m_tdoc, (ctmbstr)strBuffer );           // Parse the input
	}

	if ( rc >= 0 )
	{
		rc = tidyCleanAndRepair(this->m_tdoc);               // Tidy it up!
	}
	
	if ( rc >= 0 )
	{
		rc = tidyRunDiagnostics(this->m_tdoc);               // Kvetch
	}
	
	if ( rc > 1 )                                    // If error, force output.
	{
		rc = ( tidyOptSetBool(this->m_tdoc, TidyForceOutput, yes) ? rc : -1 );
	}
	
	if ( rc >= 0 )
	{
		rc = tidySaveFile(this->m_tdoc,(ctmbstr)strDestFileName );          // Pretty Print
	}
     
	
	m_SError=gcnew String((char*)errbuf.bp);
	tidyBufFree( &errbuf );
	strBuffer.Empty();
	m_bOperationPerformed=true;
	return true;

}
bool EfTidyNet::TidyNet::TotalWarnings (long %pVal)
{
	if(!this->m_bOperationPerformed)
	{
		m_SError = gcnew String("Please perfrom some Tidying work");
		return false;
	}
	pVal=tidyWarningCount(this->m_tdoc);
	return true;
}

bool EfTidyNet::TidyNet::TotalErrors   (long %pVal)
{
	if(!this->m_bOperationPerformed)
	{
		m_SError = gcnew String("Please perfrom some Tidying work");
		return false;
	}
	pVal=tidyErrorCount(this->m_tdoc);
	return true;

}

String^ EfTidyNet::TidyNet::ErrorWarning  ()
{
	return  m_SError;
}
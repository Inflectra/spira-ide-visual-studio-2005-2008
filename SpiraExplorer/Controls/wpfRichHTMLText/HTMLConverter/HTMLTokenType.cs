//---------------------------------------------------------------------------
// 
// File: HtmlTokenType.cs
//
// Copyright (C) Microsoft Corporation.  All rights reserved.
//
// Description: Definition of token types supported by HtmlLexicalAnalyzer
//
//---------------------------------------------------------------------------

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio.WPF.Controls
{
	public partial class wpfRichHTMLText
	{

		/// <summary>
		/// types of lexical tokens for html-to-xaml converter
		/// </summary>
		internal enum HtmlTokenType
		{
			OpeningTagStart,
			ClosingTagStart,
			TagEnd,
			EmptyTagEnd,
			EqualSign,
			Name,
			Atom, // any attribute value not in quotes
			Text, //text content when accepting text
			Comment,
			EOF,
		}
	}
}
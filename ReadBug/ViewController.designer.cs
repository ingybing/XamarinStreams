// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace ReadBug
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		UIKit.UITextField AddressBox { get; set; }

		[Outlet]
		UIKit.UITextView DebugConsole { get; set; }

		[Outlet]
		UIKit.UIButton FetchDocumentButton { get; set; }

		[Outlet]
		UIKit.UITextView OutputArea { get; set; }

		[Action ("FetchDocumentInCustomContextTouched:")]
		partial void FetchDocumentInCustomContextTouched (Foundation.NSObject sender);

		[Action ("FetchDocumentInTaskTouched:")]
		partial void FetchDocumentInTaskTouched (Foundation.NSObject sender);

		[Action ("FetchDocumentTouched:")]
		partial void FetchDocumentTouched (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (AddressBox != null) {
				AddressBox.Dispose ();
				AddressBox = null;
			}

			if (FetchDocumentButton != null) {
				FetchDocumentButton.Dispose ();
				FetchDocumentButton = null;
			}

			if (OutputArea != null) {
				OutputArea.Dispose ();
				OutputArea = null;
			}

			if (DebugConsole != null) {
				DebugConsole.Dispose ();
				DebugConsole = null;
			}
		}
	}
}

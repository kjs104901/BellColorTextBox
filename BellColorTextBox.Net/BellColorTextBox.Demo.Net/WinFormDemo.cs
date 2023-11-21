using Bell.Languages;

namespace BellColorTextBox.Demo.Net;

internal class WinFormDemo
{
    public static Thread ThreadStart()
    {
        var thread = new Thread(ThreadMain)
        {
            Name = "WinForm"
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return thread;
    }

    private static void ThreadMain()
    {
        WinFormDemoForm demoForm = new WinFormDemoForm();
        
        demoForm.TextBoxContorl.Text = SourceCodeExample.CSharp + "\n// 안녕하세요 한글 테스트";
        demoForm.TextBoxContorl.Language = Language.CSharp();

        Application.Run(demoForm);
    }
}

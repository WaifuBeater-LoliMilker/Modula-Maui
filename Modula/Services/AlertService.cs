using Microsoft.Maui.Graphics.Text;

namespace Modula.Services
{
    public interface IAlertService
    {
        public Task ShowAsync(string title, string message, string cancel);
        public Task<bool> ShowQuestionAsync(string title, string message, string accept, string cancel);
    }
    public class AlertService : IAlertService
    {
        public Task ShowAsync(string title, string message, string cancel)
        {
            var page = Application.Current?.Windows[0].Page;
            return page?.DisplayAlert(title, message, cancel) ?? Task.CompletedTask;
        }
        public Task<bool> ShowQuestionAsync(string title, string message, string accept, string cancel)
        {
            var page = Application.Current?.Windows[0].Page;
            return page?.DisplayAlert(title, message, accept, cancel) ?? Task.FromResult(false);
        }
    }
}
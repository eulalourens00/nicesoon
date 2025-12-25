using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using nicesoon.Services;
namespace nicesoon.ViewModels
{
    public class ChatMessage
    {
        public string Text { get; set; }
        public bool IsUser { get; set; }
        public DateTime Timestamp { get; set; }
        public string DisplayTime => Timestamp.ToString("HH:mm");
    }

    public class ChatViewModel : BaseViewModel
    {
        private string _userInput = "";
        private bool _isSending;
        private ApiService _apiService;
        private bool _useRealApi = false;
        public ObservableCollection<ChatMessage> Messages { get; } = new();

        public string UserInput
        {
            get => _userInput;
            set => SetProperty(ref _userInput, value);
        }

        public bool IsSending
        {
            get => _isSending;
            set => SetProperty(ref _isSending, value);
        }

        public ICommand SendMessageCommand { get; }
        public ICommand ClearChatCommand { get; }
        public ICommand SwitchToRealApiCommand { get; }

        public ChatViewModel()
        {
            _apiService = new ApiService();

            SendMessageCommand = new Command(async () => await SendMessageAsync());
            ClearChatCommand = new Command(ClearChat);
            SwitchToRealApiCommand = new Command(async () => await SwitchToRealApiAsync());

            Messages.Add(new ChatMessage
            {
                Text = "Я Найсон, твой помощник для анализа снов. Расскажи, что тебе снилось? Давай разберем вместе",
                IsUser = false,
                Timestamp = DateTime.Now
            });
        }

        private async Task SendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(UserInput))
                return;

            IsSending = true;

            try
            {
                // 1. скидываем пост
                Messages.Add(new ChatMessage
                {
                    Text = UserInput,
                    IsUser = true,
                    Timestamp = DateTime.Now
                });

                var messageText = UserInput;
                UserInput = "";

                // 2.  думаем
                var thinkingMessage = new ChatMessage
                {
                    Text = "Найсон думает...",
                    IsUser = false,
                    Timestamp = DateTime.Now
                };
                Messages.Add(thinkingMessage);

                // 3. подрубаемся
                string aiResponse;

                if (_useRealApi)
                {
                    // четко
                    aiResponse = await _apiService.SendToAIAsync(messageText);

                    // нечетко, переключаемся в тестовый режим
                    if (aiResponse.Contains("Ошибка") || aiResponse.Contains("X"))
                    {
                        _useRealApi = false;
                        aiResponse = "Ошибка API. Переключаюсь в тестовый режим.\n\n" +
                                     await _apiService.SendToAITestAsync(messageText);
                    }
                }
                else
                {
                    // ТЕСТОВЫЙ РЕЖИМ
                    aiResponse = await _apiService.SendToAITestAsync(messageText);
                }

                // 4. ответ
                Messages.Remove(thinkingMessage);

                Messages.Add(new ChatMessage
                {
                    Text = aiResponse,
                    IsUser = false,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                Messages.Add(new ChatMessage
                {
                    Text = $"Ошибка: {ex.Message}",
                    IsUser = false,
                    Timestamp = DateTime.Now
                });
            }
            finally
            {
                IsSending = false;
            }
        }

        private string _apiStatus = " Тестовый режим";
        public string ApiStatus
        {
            get => _apiStatus;
            set => SetProperty(ref _apiStatus, value);
        }

        private async Task SwitchToRealApiAsync()
        {
            IsSending = true;
            ApiStatus = " Проверка подключения...";

            try
            {
                Messages.Add(new ChatMessage
                {
                    Text = " Проверяю подключение к OpenRouter API...",
                    IsUser = false,
                    Timestamp = DateTime.Now
                });

                var isConnected = await _apiService.TestConnectionAsync();

                if (isConnected)
                {
                    _useRealApi = true;
                    ApiStatus = " Подключено к OpenRouter";

                    Messages.Add(new ChatMessage
                    {
                        Text = " Успешно! Теперь использую реальный ИИ через OpenRouter.",
                        IsUser = false,
                        Timestamp = DateTime.Now
                    });
                }
                else
                {
                    ApiStatus = " Ошибка подключения";
                    Messages.Add(new ChatMessage
                    {
                        Text = "Не удалось подключиться. Проверьте API ключ и интернет.",
                        IsUser = false,
                        Timestamp = DateTime.Now
                    });
                }
            }
            finally
            {
                IsSending = false;
            }
        }

        private void ClearChat()
        {
            Messages.Clear();
            Messages.Add(new ChatMessage
            {
                Text = "Чат очищен. Расскажите, что вам снилось?",
                IsUser = false,
                Timestamp = DateTime.Now
            });
        }
    }
}  


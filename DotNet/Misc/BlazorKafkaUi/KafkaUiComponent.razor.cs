using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using BlazorKafkaUi.Models;
using BlazorKafkaUi.State;

namespace BlazorKafkaUi.Components
{
#pragma warning disable IDE0044, IDE0051, IDE0052 // Some members can be read/updated from razor
    public partial class KafkaUiComponent : ComponentBase
    {
        [Inject] private IJSRuntime JsRuntime { get; set; }

        private readonly List<KafkaQueue> _queues = KafkaQueues.List;
        private KafkaQueue _currentQueue;
        private KafkaMessage? _currentMessage;
        private string _currentMessageTemplate;

        private IEnumerable<KafkaMessage>? _currentPageMessages;
        private int _pageSize = 25;
        private int _currentPage = 1;
        private int _totalPages = 1;
        private int _messagesTotal = 0;

        private string _searchTerm = "";
        private string _sortBy = nameof(KafkaMessage.MessageTimestamp);
        private bool _sortDesc = true;

        private string _dialogActionError = "";
        private string _dialogActionSuccess = "";

        private bool CanPrev => _currentPage > 1;
        private bool CanNext => _currentPage < _totalPages;
        private bool IsManuallyModifyableQueue => _currentQueue.Name.EndsWith("/kafka/consume");
        private bool IsViewExistingMessageDialog => _currentMessage.Topic is not null;
        private bool IsAddBtnEnabled => IsManuallyModifyableQueue && !IsViewExistingMessageDialog;
        private List<string> SelectedMessageTemplateList => KafkaQueues.GetMessageTemplateList(_currentQueue.Name);

        protected override void OnInitialized()
        {
            SelectQueue(_queues.First());
        }

        private void OnQueueChanged(ChangeEventArgs e)
        {
            string qName = e.Value?.ToString();
            if (!string.IsNullOrEmpty(qName))
                SelectQueue(_queues.Single(q => q.Name == qName));
        }

        private void SelectQueue(KafkaQueue q)
        {
            if (_currentQueue == q)
                return;
            _currentQueue = q;
            _currentPage = 1;
            ApplySearchingSortingPaging();
        }

        private void PrevPage()
        {
            if (_currentPage > 1)
                _currentPage--;
            ApplySearchingSortingPaging();
        }

        private void NextPage()
        {
            if (_currentPage < _totalPages)
                _currentPage++;
            ApplySearchingSortingPaging();
        }

        private void SortBy(string column)
        {
            if (_sortBy != column)
            {
                _sortBy = column;
                _sortDesc = true;
            }
            else
            {
                _sortDesc = !_sortDesc;
            }
            ApplySearchingSortingPaging();
        }

        private void ApplySearchingSortingPaging()
        {
            List<KafkaMessage> messages;
            lock (_currentQueue.Lock)
                messages = _currentQueue.Messages.ToList();

            if (!string.IsNullOrWhiteSpace(_searchTerm))
            {
                string s = _searchTerm.Trim().ToLowerInvariant();
                messages = messages.Where(m =>
                    (m.MessageKey ?? "").ToLowerInvariant().Contains(s) ||
                    (m.MessageValue ?? "").ToLowerInvariant().Contains(s)
                ).ToList();
            }

            messages = _sortBy switch
            {
                nameof(KafkaMessage.Offset) => (_sortDesc ? messages.OrderByDescending(m => m.Offset) : messages.OrderBy(m => m.Offset)).ToList(),
                _ => (_sortDesc ? messages.OrderByDescending(m => m.MessageTimestamp) : messages.OrderBy(m => m.MessageTimestamp)).ToList(),
            };

            _messagesTotal = messages.Count();
            _totalPages = (_messagesTotal + _pageSize - 1) / _pageSize;
            if (_currentPage > _totalPages)
                _currentPage = _totalPages;
            if (_currentPage < 1)
                _currentPage = 1;

            _currentPageMessages = messages.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();
        }

        private MarkupString SortIndicator(string column)
        {
            return _sortBy != column
                ? (MarkupString)""
                : (MarkupString)(_sortDesc ? "▼" : "▲");
        }

        private string Shorten(string? s, int max)
        {
            if (string.IsNullOrEmpty(s))
                return "";
            if (s.Length <= max)
                return s;
            return s[..(max - 3)] + "...";
        }

        private void OpenAddNewMessageDialog()
        {
            OpenMessageDialog(new KafkaMessage());
        }

        private void OpenMessageDialog(KafkaMessage message)
        {
            ClearDialogActionText();
            _currentMessage = message;
        }

        private void CloseMessageDialog()
        {
            _currentMessage = null;
            ApplySearchingSortingPaging();
        }

        private async Task CopyMessageValueToClipboard()
        {
            ClearDialogActionText();
            if (_currentMessage is null || string.IsNullOrEmpty(_currentMessage.MessageValue))
            {
                _dialogActionError = "Message value is empty.";
                return;
            }
            await JsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", _currentMessage.MessageValue);
            _dialogActionSuccess = "Message value copied.";
        }

        private void AddMessage()
        {
            ClearDialogActionText();

            if (string.IsNullOrEmpty(_currentMessage.MessageValue))
            {
                _dialogActionError = "Message value is empty.";
                return;
            }

            long offset;
            lock (_currentQueue.Lock)
            {
                offset = ++_currentQueue.LastOffset;
                _currentQueue.Messages.Enqueue(new KafkaMessage()
                {
                    Topic = _currentQueue.Name,
                    Partition = 1,
                    Offset = offset,
                    MessageKey = string.IsNullOrEmpty(_currentMessage.MessageKey) ? null : _currentMessage.MessageKey,
                    MessageValue = _currentMessage.MessageValue,
                    MessageTimestamp = DateTime.UtcNow,
                });
            }
            _dialogActionSuccess = $"Message added (offset={offset}).";
            _currentMessage = new KafkaMessage();
            _currentMessageTemplate = null;
        }

        private void ClearDialogActionText()
        {
            _dialogActionError = "";
            _dialogActionSuccess = "";
        }

        private void OnMessageTemplateSelected(ChangeEventArgs e)
        {
            string tName = e.Value?.ToString();
            _currentMessageTemplate = tName;
            if (!string.IsNullOrEmpty(tName))
                _currentMessage.MessageValue = KafkaQueues.CreateMessageTemplate(_currentQueue.Name, tName);
        }
    }
}

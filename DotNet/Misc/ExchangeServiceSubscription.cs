		private async Task SomeFunction()
		{
			try
			{
				var StreamingSubscription = await this.exchangeService.SubscribeToStreamingNotifications(
                    new FolderId[] { WellKnownFolderName.Inbox },
                    EventType.NewMail,
                    EventType.Created,
                    EventType.Deleted,
                    EventType.Modified,
                    EventType.Moved,
                    EventType.Copied,
                    EventType.FreeBusyChanged);
                // Create a streaming connection to the service object, over which events are returned to the client.
                // Keep the streaming connection open for 30 minutes.
                StreamingSubscriptionConnection connection = new StreamingSubscriptionConnection(this.exchangeService, 30);
                connection.AddSubscription(StreamingSubscription);
                connection.OnNotificationEvent += OnNotificationEvent;
                connection.OnDisconnect += OnDisconnect;
                connection.Open();

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void OnDisconnect(object sender, SubscriptionErrorEventArgs args)
        {
            Logger.Trace($"Disconnected subscription with id={args.Subscription.Id});
        }

        private void OnNotificationEvent(object sender, NotificationEventArgs args)
        {
            foreach (var notificationEvent in args.Events)
            {
                switch (notificationEvent.EventType)
                {
                    case EventType.Status:
                        Logger.Trace($"{notificationEvent.EventType} {((ItemEvent)notificationEvent).ItemId} {((ItemEvent)notificationEvent).ParentFolderId}");
                        break;
                    case EventType.NewMail:
                        Logger.Trace($"{notificationEvent.EventType} {((ItemEvent)notificationEvent).ItemId} {((ItemEvent)notificationEvent).ParentFolderId}");
                        break;
                    case EventType.Created:
                        Logger.Trace($"{notificationEvent.EventType} {((ItemEvent)notificationEvent).ItemId} {((ItemEvent)notificationEvent).ParentFolderId}");
                        break;
                    case EventType.Moved:
                        Logger.Trace($"{notificationEvent.EventType} {((ItemEvent)notificationEvent).ItemId} {((ItemEvent)notificationEvent).ParentFolderId}");
                        break;
                    case EventType.Modified:
                        Logger.Trace($"{notificationEvent.EventType} {((ItemEvent)notificationEvent).ItemId} {((ItemEvent)notificationEvent).ParentFolderId}");
                        break;
                    case EventType.Copied:
                        Logger.Trace($"{notificationEvent.EventType} {((ItemEvent)notificationEvent).ItemId} {((ItemEvent)notificationEvent).ParentFolderId}");
                        break;
                    case EventType.FreeBusyChanged:
                        Logger.Trace($"{notificationEvent.EventType} {((ItemEvent)notificationEvent).ItemId} {((ItemEvent)notificationEvent).ParentFolderId}");
                        break;
                    case EventType.Deleted:
                        Logger.Trace($"{notificationEvent.EventType} {((ItemEvent)notificationEvent).ItemId} {((ItemEvent)notificationEvent).ParentFolderId}");
                        break;
                    default:
                        break;
                }
            }
        }
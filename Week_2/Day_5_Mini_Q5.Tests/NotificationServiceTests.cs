// Each test checks one specific behavior of NotificationService:
//   1. The delegate we passed in actually gets called with the right data.
//   2. The OnNotificationSent event actually fires, and carries the
//      correct Recipient/Message.
//   3. Firing the event with ZERO subscribers doesn't crash (this is what
//      the `?.Invoke` null-check in NotificationService.cs protects
//      against - if that check were missing, this test would fail).
//   4. MULTIPLE subscribers all get notified, not just the first one.
//   5-6. NotificationLog itself works correctly, both on its own and when
//      wired up to a real NotificationService.
public class NotificationServiceTests
{
    [Fact]
    public void Send_InvokesUnderlyingSenderDelegate()
    {
        string? sentRecipient = null;
        string? sentMessage = null;
        var service = new NotificationService((recipient, message) =>
        {
            sentRecipient = recipient;
            sentMessage = message;
        });

        service.Send("asha@example.com", "hello");

        Assert.Equal("asha@example.com", sentRecipient);
        Assert.Equal("hello", sentMessage);
    }

    [Fact]
    public void Send_RaisesOnNotificationSent_WithRecipientAndMessage()
    {
        var service = new NotificationService((_, _) => { });
        NotificationSentEventArgs? received = null;
        service.OnNotificationSent += (_, e) => received = e;

        service.Send("asha@example.com", "hello");

        Assert.NotNull(received);
        Assert.Equal("asha@example.com", received!.Recipient);
        Assert.Equal("hello", received.Message);
    }

    [Fact]
    public void Send_NoSubscribers_DoesNotThrow()
    {
        var service = new NotificationService((_, _) => { });
        // Record.Exception runs the given code and returns whatever
        // exception it threw (or null if nothing was thrown) - a clean way
        // to assert "this should NOT throw" without wrapping everything in
        // a manual try/catch.
        var exception = Record.Exception(() => service.Send("asha@example.com", "hello"));
        Assert.Null(exception);
    }

    [Fact]
    public void Send_MultipleSubscribers_AllReceiveTheEvent()
    {
        var service = new NotificationService((_, _) => { });
        var callCount = 0;
        service.OnNotificationSent += (_, _) => callCount++;
        service.OnNotificationSent += (_, _) => callCount++;

        service.Send("asha@example.com", "hello");

        Assert.Equal(2, callCount);
    }

    [Fact]
    public void NotificationLog_Record_AppendsFormattedEntry()
    {
        var log = new NotificationLog();

        log.Record(null, new NotificationSentEventArgs("asha@example.com", "hello"));

        Assert.Single(log.Entries);
        Assert.Equal("Sent to asha@example.com: hello", log.Entries[0]);
    }

    [Fact]
    public void NotificationLog_SubscribedToService_RecordsOnSend()
    {
        var log = new NotificationLog();
        var service = new NotificationService(EmailSender.Send);
        service.OnNotificationSent += log.Record;

        service.Send("asha@example.com", "hello");
        service.Send("rohit@example.com", "world");

        Assert.Equal(2, log.Entries.Count);
    }
}

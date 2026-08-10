// One shared NotificationLog, subscribed to three SEPARATE
// NotificationService instances - each one constructed with a different
// sender delegate (Email/Sms/Push). This proves the log doesn't care which
// channel actually delivered the message; it just listens for the event.
var log = new NotificationLog();

var emailService = new NotificationService(EmailSender.Send);
emailService.OnNotificationSent += log.Record;
emailService.Send("asha@example.com", "Your order has shipped.");

var smsService = new NotificationService(SmsSender.Send);
smsService.OnNotificationSent += log.Record;
smsService.Send("+1-555-0100", "OTP: 483920");

var pushService = new NotificationService(PushSender.Send);
pushService.OnNotificationSent += log.Record;
pushService.Send("device-42", "You have a new message.");

Console.WriteLine();
Console.WriteLine($"Total logged notifications: {log.Entries.Count}");

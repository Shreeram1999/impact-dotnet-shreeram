// All three objects are stored using the base type `Notification`, but
// calling Send() on each one still runs ITS OWN override - including the
// sealed one on EmailNotification. Sealing only stops FURTHER classes from
// overriding Send() again; it doesn't change how Send() behaves right now.
Notification[] notifications =
[
    new EmailNotification(),
    new SmsNotification(),
    new PushNotification(),
];

foreach (var notification in notifications)
    notification.Send("Your order has shipped.");

Console.WriteLine();
Console.WriteLine("See Notification.cs for the commented-out sealed-override");
Console.WriteLine("attempt and the CS0239 compile error it produces.");

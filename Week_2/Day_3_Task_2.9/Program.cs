var alarmClock = new AlarmClock();
var person = new Person("Asha");
var coffeeMachine = new CoffeeMachine();

// `+=` is how you "subscribe" a method to an event - it's saying "when
// OnAlarmRing fires, please also call this method". Both person.WakeUp and
// coffeeMachine.StartBrewing get added here, so when the alarm rings, both
// of them will run, one after the other, in the order they were added.
alarmClock.OnAlarmRing += person.WakeUp;
alarmClock.OnAlarmRing += coffeeMachine.StartBrewing;

// This is the moment the event actually fires - RingAlarm() internally
// calls OnAlarmRing?.Invoke(...), which runs every subscribed method with
// the same AlarmEventArgs (so they all see the same alarm time).
var alarmTime = new DateTime(2026, 8, 9, 7, 0, 0);
alarmClock.RingAlarm(alarmTime);

# Myxomatosis

Myxomatosis is a client for Rabbit MQ designed to convert a stream of events on a queue to an observable sequence.  

To create a connection to a RabbitMQ server running locally:

	var connection = ObservableConnectionFactory.Create();

To publish a strongly typed message to a queue:

	connection
	    .GetQueue<MyMessage>("MessageExchange", "MessageQueue")
	    .Publish(new MyMessage {Greeting = "Hello from publisher"});

To create an observable stream of representing the stream of messages on the queue:

    connection
        .GetQueue<MyMessage>("MessageExchange", "MessageQueue")
        .Listen()
        .ToObservable()
        .SubscribeWithAck(rm => Console.WriteLine("Recieved message: {0}", rm.Message.Greeting));

As a more complicated example, suppose we have the following constraints:

* We can process up to 100 messages a second;
* **Special** messages (identified by a flag in the message header) can be processed in batches of 25 up to 4 times a second;
* Normal messages must be processed individually and we can only process 50 of these per second.

We can achieve this simply by applying reactive extensions to the observable stream:

	var smoothedStream = connection.GetQueue<MyMessage>("MessageExchange", "MessageQueue")
	    .Listen()
	    .ToObservable()
	    .Pace(TimeSpan.FromMilliseconds(10));

	Func<RabbitMessage, bool> streamSplitter = m => m.RawHeaders.ContainsKey("IsSpecial");

	smoothedStream.Where(rm => streamSplitter(rm))
	    .Buffer(TimeSpan.FromSeconds(0.25), 25)
	    .Select(b => b.AsEnumerable())
	    .SubscribeWithAck(rms =>
	    {
	        foreach (var rabbitMessage in rms)
	        {
	            Console.WriteLine("Recieved message: {0}", rabbitMessage.Message.Greeting);
	        }
	    });

	smoothedStream.Where(rm => !streamSplitter(rm))
	    .Pace(TimeSpan.FromSeconds(20))
	    .SubscribeWithAck(rm => Console.WriteLine("Boring message recieved: {0}", rm.Message.Greeting));







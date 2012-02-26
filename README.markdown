##NHibernate.ZMQLogPublisher##

Project started as part of an effort to provide an open source NHibernate profiler. This library will provide the log details to be analyzed by a client application to view the data.

##Limitations##
ZMQLogger is in its very early stages and there are some limitations which I haven't gotten around sorting out yet.

* The solution is only built with x86 support at the moment, if you want x64 support, you would need to change the target platform for the project and replace the clrzmq package with clrzmq-x64.
* Uses IInternalLogger interface which introduced in NH3, depending on how things go, I might add an implementaion for log4net so that it would work with previous version of NH.
* Built against NH3.2. Besides the above limitation, the code is built against NH3.2 so you might get runtime exceptions if you are using a different version of NHibernate, but I doubt that IInternalLogger interface will change often, so a binding redirect should do the trick.

##Usage##

1. Add dependencies to your solution, easiest way is via nuget console:

        install-package clrzmq
		install-package servicestack.text

2. Downlaod NHibernate.ZMQLogPublisher.dll and add it as a reference in your project.

3. At the start of  you application add a call to

        NHibernate.ZMQLogPublisher.Publisher.Start();

    > By default port 68748 is used, if you want to change this, pass a port number to the Start method call.

4. Create a project a subscriber project, add dependencies to it, and start listening for messages:

            using(Context context = new Context(1))
            using (Socket subscriber = context.Socket(SocketType.SUB))
            {
                subscriber.Connect("tcp://localhost:68748");
                subscriber.Subscribe("", Encoding.Unicode);

                while (!message.Contains("unbinding factory"))
                {
                    message = subscriber.Recv(Encoding.Unicode);
                    
                    Console.WriteLine(message);
                }
            } 

If you just want to see it working, start the test publisher and test subscriber projects in the source code.

##Licence##

Code is released under the [new bsd licence](licence.txt)

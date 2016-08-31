#region

using GMap.NET;
using POGOProtos.Data;
using POGOProtos.Map.Fort;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#endregion


namespace PokemonGo.RocketAPI.Logic.Logging
{
    public delegate void MessageEventHandler(object sender, MessageEventArgs a);

    public class LogicEvents
    {

        // Declare the event using EventHandler<T>
        public event EventHandler<MessageEventArgs> RaiseMessageEvent;
        public event EventHandler<MessageEventArgs> UpdateTitleEvent;
        public event EventHandler<PokemonDataCollectionEventArgs> EvolvePokemonsEvent;
        public event EventHandler<FortDataCollectionEventArgs> FortsChangedEvent;
        public event EventHandler<PointLatLngEventArgs> PlayerPositionChangedEvent;

        public void WriteMessage(string message)
        {
            // Write some code that does something useful here
            // then raise the event. You can also raise an event
            // before you execute a block of code.
            OnRaiseMessageEvent(new MessageEventArgs(message));

        }

        public void RaiseUpdateTitleEvent(string message)
        {
            // Write some code that does something useful here
            // then raise the event. You can also raise an event
            // before you execute a block of code.
            OnUpdateTitleEvent(new MessageEventArgs(message));

        }
        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void OnRaiseMessageEvent(MessageEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<MessageEventArgs> handler = RaiseMessageEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Format the string to send inside the CustomEventArgs parameter
                // e.Message += String.Format(" at {0}", DateTime.Now.ToString());

                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void OnUpdateTitleEvent(MessageEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<MessageEventArgs> handler = UpdateTitleEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Format the string to send inside the CustomEventArgs parameter
                e.Message += String.Format(" at {0}", DateTime.Now.ToString());

                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        public void RaiseEvolvePokemonEvent(IEnumerable<PokemonData> pokemonList)
        {
            // Write some code that does something useful here
            // then raise the event. You can also raise an event
            // before you execute a block of code.
            OnEvolvePokemonEvent(new PokemonDataCollectionEventArgs(pokemonList));

        }
        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void OnEvolvePokemonEvent(PokemonDataCollectionEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<PokemonDataCollectionEventArgs> handler = EvolvePokemonsEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }



        public void RaiseFortsChangedEvent(IEnumerable<FortData> FortList)
        {
            // Write some code that does something useful here
            // then raise the event. You can also raise an event
            // before you execute a block of code.
            OnFortsChangedEvent(new FortDataCollectionEventArgs(FortList));

        }
        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void OnFortsChangedEvent(FortDataCollectionEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<FortDataCollectionEventArgs> handler = FortsChangedEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }


        public void RaisePlayerPositionChangedEvent(PointLatLng newPosition)
        {
            // Write some code that does something useful here
            // then raise the event. You can also raise an event
            // before you execute a block of code.
            OnPlayerPositionChangedEvent(new PointLatLngEventArgs(newPosition));

        }
        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void OnPlayerPositionChangedEvent(PointLatLngEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<PointLatLngEventArgs> handler = PlayerPositionChangedEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

    }
    /// <summary>
    /// Generic logger which can be used across the projects.
    /// Logger should be set to properly log.
    /// </summary>
    public static class Logger
    {
        private static string _currentFile = string.Empty;
        private static readonly string Path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Logs");
        //private static Logger _logger;
        public static LogicEvents Events =  new LogicEvents();

        /// <summary>
        /// Set the logger. All future requests to <see cref="Write(string,LogLevel,ConsoleColor)"/> will use that logger, any old will be unset.
        /// </summary>
        public static void SetLogger()
        {
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }
            _currentFile = DateTime.Now.ToString("yyyy-MM-dd - HH.mm.ss");
            Log($"Initializing Rocket logger @ {DateTime.Now}...");
        }

        /// <summary>
        ///     Log a specific message to the logger setup by <see cref="SetLogger()" /> .
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="level">Optional level to log. Default <see cref="LogLevel.Info" />.</param>
        /// <param name="color">Optional. Default is automatic color.</param>
        public static void Write(string message, LogLevel level = LogLevel.None, ConsoleColor color = ConsoleColor.White)
        {
            Console.OutputEncoding = Encoding.Unicode;

            string outputMessage = "";
                       
            
            var dateFormat = DateTime.Now.ToString("HH:mm:ss");
            if (Logic._client != null && Logic._client.Settings.DebugMode)
                dateFormat = DateTime.Now.ToString("HH:mm:ss:fff");

            switch (level)
            {
                case LogLevel.Info:
                    outputMessage = ($"[{dateFormat}] (INFO) {message}");
                    break;
                case LogLevel.Warning:
                    outputMessage = ($"[{dateFormat}] (ATTENTION) {message}");
                    break;
                case LogLevel.Error:
                    outputMessage = ($"[{dateFormat}] (ERROR) {message}");
                    break;
                case LogLevel.Debug:
                    if (Logic._client.Settings.DebugMode)
                    {
                        outputMessage = ($"[{dateFormat}] (DEBUG) {message}");
                    }
                    break;
                case LogLevel.Navigation:
                    outputMessage = ($"[{dateFormat}] (NAVIGATION) {message}");
                    break;
                case LogLevel.Pokestop:
                    outputMessage = ($"[{dateFormat}] (POKESTOP) {message}");
                    break;
                case LogLevel.Pokemon:
                    outputMessage = ($"[{dateFormat}] (PKMN) {message}");
                    break;
                case LogLevel.Transfer:
                    outputMessage = ($"[{dateFormat}] (TRANSFER) {message}");
                    break;
                case LogLevel.Evolve:
                    outputMessage = ($"[{dateFormat}] (EVOLVE) {message}");
                    break;
                case LogLevel.Berry:
                    outputMessage = ($"[{dateFormat}] (BERRY) {message}");
                    break;
                case LogLevel.Egg:
                    outputMessage = ($"[{dateFormat}] (EGG) {message}");
                    break;
                case LogLevel.Incense:
                    outputMessage = ($"[{dateFormat}] (INCENSE) {message}");
                    break;
                case LogLevel.Recycling:
                    outputMessage = ($"[{dateFormat}] (RECYCLING) {message}");
                    break;
                case LogLevel.Incubation:
                    outputMessage = ($"[{dateFormat}] (INCUBATION) {message}");
                    break;
                case LogLevel.None:
                    outputMessage = ($"[{dateFormat}] {message}");
                    break;
                default:
                    outputMessage = ($"[{dateFormat}] {message}");
                    break;
            }

            if (outputMessage.Length > 0)
            {
                Log(outputMessage);
                Events.WriteMessage(outputMessage);
            }

        }

        public static void UpdateTitle(string message)
        {
            Events.RaiseUpdateTitleEvent(message);
        }

        private static void Log(string message)
        {
            // maybe do a new log rather than appending?
            using (var log = File.AppendText(System.IO.Path.Combine(Path, _currentFile + ".txt")))
            {
                log.WriteLine(message);
                log.Flush();
            }
        }
    }

    public enum LogLevel
    {
        None = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Debug = 4,
        Navigation = 5,
        Pokestop = 6,
        Pokemon = 7,
        Transfer = 8,
        Evolve = 9,
        Berry = 10,
        Egg = 11,
        Incense = 12,
        Recycling = 13,
        Incubation = 14
    }
}
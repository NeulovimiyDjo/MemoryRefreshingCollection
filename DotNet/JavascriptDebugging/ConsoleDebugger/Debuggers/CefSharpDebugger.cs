using CommandLine;
using CommunicatorLib;
using CommunicatorLib.Messages;
using Newtonsoft.Json;
using Nito.AsyncEx;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleDebugger.Debuggers
{
    public class BreakOptions
    {
        [Value(0, Required = true)]
        public int LineNumber { get; set; }
    }
    public class EvalOptions
    {
        [Value(0, Required = true)]
        public string Expression { get; set; }
    }
    public class SetOptions
    {
        [Value(0, Required = true)]
        public string VariableName { get; set; }

        [Value(1, Required = true)]
        public string NewValue { get; set; }
    }


    public class CefSharpDebugger
    {
        private static readonly TimeSpan WebsocketConnectTimeout = TimeSpan.FromSeconds(99999);

        private Parser _cmdParser = new Parser();

        private static AsyncLock _ioAsyncLock = new AsyncLock();
        private static AsyncManualResetEvent _breakWaiterManualResetEvent = new AsyncManualResetEvent(false);
        private static CancellationTokenSource _breakWaiterCancellationTokenSource = new CancellationTokenSource();

        private string _scriptId = null;
        private string _scriptSource = null;
        private string _callFrameId = null;



        public async Task StartAsync()
        {
            using (var communicator = new Communicator("localhost", 8881))
            {
                await ConnectAndLoadScriptAsync(communicator);
                var breakMonitorTask = Task.Run(async () => await RunBreakWaiterAsync(communicator));

                string cmdWithArgs = Console.In.ReadLine();
                while (cmdWithArgs != "exit")
                {
                    try
                    {
                        await ProcessCommandAsync(communicator, cmdWithArgs);
                    }
                    catch (Exception ex)
                    {
                        await WriteMessageAsync(ex.Message);
                    }

                    cmdWithArgs = Console.In.ReadLine();
                }

                try
                {
                    _breakWaiterCancellationTokenSource.Cancel();
                    await breakMonitorTask;
                }
                catch (OperationCanceledException) { }
                await communicator.Disconnect(new CancellationTokenSource(WebsocketConnectTimeout).Token);
            }
        }


        private async Task ProcessCommandAsync(Communicator communicator, string cmdWithArgs)
        {
            var cmd = cmdWithArgs.Split(" ")[0];
            switch (cmd)
            {
                case "break":
                    var breakOptions = ParseOptions<BreakOptions>(cmdWithArgs);
                    await BreakAsync(communicator, breakOptions.LineNumber - 1);
                    break;
                case "eval":
                    var evalOptions = ParseOptions<EvalOptions>(cmdWithArgs);
                    await EvalAsync(communicator, evalOptions.Expression);
                    break;
                case "set":
                    var setOptions = ParseOptions<SetOptions>(cmdWithArgs);
                    await SetAsync(communicator, setOptions.VariableName, setOptions.NewValue);
                    break;
                case "next":
                    await NextAsync(communicator);
                    break;
                case "continue":
                    await ContinueAsync(communicator);
                    break;
                default:
                    await WriteMessageAsync($"Invalid command '{cmd}'");
                    break;
            }
        }


        #region CommandsImpl

        private async Task BreakAsync(Communicator communicator, int lineNumber)
        {
            var commandResult = await communicator.SendCommand("Debugger.setBreakpoint", new { location = new Location { LineNumber = lineNumber, ColumnNumber = 0, ScriptId = _scriptId } });
            var breakpointResult = JsonConvert.DeserializeObject<V8CommandResponse<SetBreakpointResponse>>(commandResult);
            await WriteMessageAsync($"Breakpoint is set, breakpointId = {breakpointResult.Result.BreakpointId}");
        }


        private async Task EvalAsync(Communicator communicator, string expression)
        {
            if (_callFrameId == null)
            {
                await WriteMessageAsync("No breakpoint have been hit yet");
                return;
            }


            var commandResult = await communicator.SendCommand("Debugger.evaluateOnCallFrame", new { callFrameId = _callFrameId, expression = expression });
            var evaluateResult = JsonConvert.DeserializeObject<V8CommandResponse<EvaluateOnCallFrameResponse>>(commandResult);
            await WriteMessageAsync($"type: {evaluateResult.Result.Result.ObjectType}, value: {evaluateResult.Result.Result.Value}");
        }


        private async Task SetAsync(Communicator communicator, string variableName, object newValue)
        {
            if (_callFrameId == null)
            {
                await WriteMessageAsync("No breakpoint have been hit yet");
                return;
            }


            await communicator.SendCommand("Debugger.setVariableValue", new { scopeNumber = 0, variableName = variableName, newValue = new { value = newValue }, callFrameId = _callFrameId });
            await WriteMessageAsync($"{variableName} was set to {newValue}");
        }


        private async Task NextAsync(Communicator communicator)
        {
            if (_callFrameId == null)
            {
                await WriteMessageAsync("No breakpoint have been hit yet");
                return;
            }


            await communicator.SendCommand("Debugger.stepOver");
            await WaitBreakAsync(communicator);
        }


        private async Task ContinueAsync(Communicator communicator)
        {
            if (_callFrameId == null)
            {
                await WriteMessageAsync("No breakpoint have been hit yet");
                return;
            }


            await WriteMessageAsync("Resuming debugger");
            await communicator.SendCommand("Debugger.resume");
            _callFrameId = null;

            _breakWaiterManualResetEvent.Set(); // Resume waiting for breakpoints.
        }

        #endregion


        #region OtherHelperFunctions

        private async Task RunBreakWaiterAsync(Communicator communicator)
        {
            while (true)
            {
                await WriteMessageAsync("Waiting for breakpoint");
                await WaitBreakAsync(communicator);

                // Debugger now is in paused state, wait for it to resume.
                _breakWaiterManualResetEvent.Reset();
                await _breakWaiterManualResetEvent.WaitAsync();
            }
        }


        private async Task WaitBreakAsync(Communicator communicator)
        {
            var debuggerPausedEvent = await communicator.WaitForEventAsync<DebuggerPausedEvent>(_breakWaiterCancellationTokenSource.Token);
            int pausedLine = debuggerPausedEvent.CallFrames[0].Location.LineNumber;
            await WriteMessageAsync($"{GetFormattedScriptSource(pausedLine)}");
            await WriteMessageAsync($"Breakpoint was hit at line {pausedLine + 1}\n");

            _callFrameId = debuggerPausedEvent.CallFrames[0].CallFrameId;
        }


        private async Task ConnectAndLoadScriptAsync(Communicator communicator)
        {
            await communicator.Connect(new CancellationTokenSource(WebsocketConnectTimeout).Token);

            // https://chromedevtools.github.io/devtools-protocol/1-2/Debugger/

            await communicator.SendCommand("Runtime.enable");
            await communicator.SendCommand("Debugger.enable");
            //await communicator.SendCommand("Page.enable");
            //await communicator.SendCommand("Target.setDiscoverTargets", new { discover = true });
            //await communicator.SendCommand("Target.getTargets", new { discover = true });
            //await communicator.SendCommand("Page.reload");
            //await communicator.SendCommand("Runtime.runIfWaitingForDebugger");



            // After enabling debugging? we should get both a ScriptParsed event & a DebuggerPaused event.
            var scriptParsedEvent = await communicator.WaitForEventAsync<ScriptParsedEvent>(new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);
            _scriptId = scriptParsedEvent.ScriptId;

            var commandResult = await communicator.SendCommand("Debugger.getScriptSource", new { scriptId = _scriptId });
            var scriptSourceResponse = JsonConvert.DeserializeObject<V8CommandResponse<GetScriptSourceResponse>>(commandResult);
            _scriptSource = scriptSourceResponse.Result.ScriptSource;
            await WriteMessageAsync($"Debugging script:\n\n{GetFormattedScriptSource()}");
        }


        private string GetFormattedScriptSource(int pausedLineZeroBased = -1)
        {
            string res = "";

            var lines = _scriptSource.Replace("\r","").Split("\n").ToList();
            int currentLine = 1;
            lines.ForEach(line =>
            {
                string prefix = currentLine == pausedLineZeroBased + 1 ? "=>" : "  ";
                res += $"{currentLine}:{prefix} {line}\n";
                currentLine++;
            });

            return res;
        }


        private T ParseOptions<T>(string cmdWithArgs) where T : class
        {
            return _cmdParser
                .ParseArguments<T>(cmdWithArgs.Split(" ").Skip(1))
                .MapResult(
                    options => options,
                    errors =>
                    {
                        throw new Exception($"Bad arguments {string.Join(" ", errors.ToList().Select(err => err.Tag))}.");
                    }
                );
        }


        private async Task WriteMessageAsync(string msg)
        {
            using (await _ioAsyncLock.LockAsync())
            {
                await Console.Out.WriteLineAsync(msg);
            }
        }

        #endregion
    }
}

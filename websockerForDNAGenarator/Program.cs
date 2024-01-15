using System.Net;
using System.Net.WebSockets;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseUrls("http://localhost:5010");
        var app = builder.Build();

        app.UseWebSockets();

        app.Map("/dna", async context =>
        {
            if(context.WebSockets.IsWebSocketRequest)
            {
                using var ws = await context.WebSockets.AcceptWebSocketAsync();
                bool statusNotStop = false;
                StringBuilder codonToCheck = new StringBuilder();

                while(true)
                {
                    while (!statusNotStop)
                    {
                        var message = GenNucleotid().ToString();
                        //char nucleotid = GenNucleotid();
                        //Console.WriteLine(message);
                        //Thread.Sleep(1000);
                        codonToCheck.Append(message);
                        statusNotStop = CheckStopCodon(codonToCheck, statusNotStop);
                        var bytes = Encoding.UTF8.GetBytes(message);

                        var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);

                        if (ws.State == WebSocketState.Open)
                        {
                            await ws.SendAsync(arraySegment, WebSocketMessageType.Text,
                                true, CancellationToken.None);
                            if (statusNotStop)
                            {
                                var newMessage = "Generartion end because The StopCodon " + codonToCheck.ToString() + " was Generate";
                                bytes = Encoding.UTF8.GetBytes(newMessage);
                                arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
                                await ws.SendAsync(arraySegment, WebSocketMessageType.Text,
                                    true, CancellationToken.None);
                                codonToCheck.Clear();
                                break;
                            }
                        }
                        else if (ws.State == WebSocketState.Closed || ws.State == WebSocketState.Aborted)
                        {
                            break;
                        }
                        Thread.Sleep(1000);
                    }
                    
                }

            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        });

        app.Run();
    }

    private static char GenNucleotid()
    {
        string nucleotidCharacter = "ATCG";
        Random generator = new Random();
        int index = generator.Next(0, nucleotidCharacter.Length);
        char generatedNucleaotid = nucleotidCharacter[index];
        return generatedNucleaotid;
    }

    private static bool CheckStopCodon(StringBuilder codon, bool checker)
    {
        if (codon.Length == 3)
        {
            if (codon.Equals("TAA") || codon.Equals("TAG") || codon.Equals("TGA"))
            {
                checker = true;
            }
            else
            {
                codon.Remove(0, 1);
            }
        }
        return checker;
    }
}
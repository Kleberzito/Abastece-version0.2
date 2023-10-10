using System.Net;
using System;
using Renci.SshNet;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
//using ThreadNetwork;

namespace Abastece_version0._2
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnButton(object sender, EventArgs e)
        {
            string host = txtIP.Text;

            txtIP.Text = "";
            lblAlert.Text = "";

            StatusLoader(Loader, Start, "", true);

            await Task.Delay(3000);

            ValidateSSH(host, lblAlert);

            StatusLoader(Loader, Start, "Iniciar", false);
        }

        private void ConnectSSH(string host, string user, string password, Label lbl)
        {
            try
            {
                using (var client = new SshClient(host, user, password))
                {
                    client.Connect();

                    if (client.IsConnected)
                    {
                        //lbl.Text = RangerIp().ToString();
                        //ValidateService(client, "openvpn", lbl);
                        //ValidateEqui(client, RangerIp().ToString(), lbl);                         

                    }
                    else
                    {
                        lbl.Text = "Não foi possivel estabelecer uma conexão com o Servidor";
                    }
                }
            }
            catch (Exception ex)
            {
                lbl.Text = $"Ocorreu um erro ao conectar-se ao servidor SSH: {ex.Message}";
            }
        }

        private string RangerIp()
        {
            int[] ip_ranger = { 1, 2, 212 };

            for (int i = 0; i < ip_ranger.Length; i++)
            {
                if (pingIP($"192.168.{ip_ranger[i]}.105"))
                {
                    return $"192.168.{ip_ranger[i]}.105";
                }
            }

            return "Falha IP";
        }

        private void ValidateSSH(string ip, Label lbl)
        {
            string[] newIP = ip.Split('.');
            int nIP = int.Parse(newIP[3]);

            string serv = ip;
            string link = $"{newIP[0]}.{newIP[1]}.{newIP[2]}.{nIP - 1}";


            if (pingIP(serv))
            {
                //lbl.Text = $"Ping {serv} SERVIDOR bem-sucedido!";

                string username = "kleber";
                string password = "root";

                ConnectSSH(ip, username, password, lblAlert);
            }
            else
            {
                if (pingIP(link))
                {
                    lbl.Text = $"Ping {serv} falha no SERVIDOR, ping {link} LINK bem-sucedido!";
                }
                else
                {
                    lbl.Text = $"FALHA RESPOSTA: {serv} e {link} Ping falhou.";
                }
            }
        }

        private bool pingIP(string ipAddress)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    //Enviara o comando de ping e validar a resposta
                    PingReply reply = ping.Send(ipAddress);

                    //Valida qual resposta tivemos do processo de ping
                    if (reply.Status == IPStatus.Success)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (PingException)
            {
                return false;
                // Trate exceções de ping, se necessário
            }
        }

        private void ValidateService(SshClient client, string serv, Label lbl)
        {
            string command = $"systemctl is-active {serv}.service";

            var cmd = client.RunCommand(command);

            lbl.Text = $"O serviço esta: {cmd.Result}";
        }

        private void ValidateEqui(SshClient client, string ip, Label lbl)
        {
            //string command = $"ping -c 4 192.168.1.{finalIP}";

            string command = $"ping -c 4 {ip}";

            var cmd = client.RunCommand(command);

            string result = cmd.Result.ToString();

            string pattern = @"(\d+) packets transmitted, (\d+) received, (\d+)% packet loss, time (\d+)ms\nrtt min/avg/max/mdev = (\d+\.\d+)/(\d+\.\d+)/(\d+\.\d+)/(\d+\.\d+) ms";

            Match match = Regex.Match(result, pattern);

            if (match.Success)
            {
                int packetsTransmitted = int.Parse(match.Groups[1].Value);
                int received = int.Parse(match.Groups[2].Value);
                int packetLoss = int.Parse(match.Groups[3].Value);
                int timeMs = int.Parse(match.Groups[4].Value);
                double rttMin = double.Parse(match.Groups[5].Value);
                double rttAvg = double.Parse(match.Groups[6].Value);
                double rttMax = double.Parse(match.Groups[7].Value);
                double rttMdev = double.Parse(match.Groups[8].Value);


                if (packetsTransmitted != 0)
                {
                    if (received > 0)
                    {
                        lbl.Text = "Equipamento comunicando: OK";
                        //lbl.Text = $"resposta mínimo: {rttMin} ms, Médio: {rttAvg} ms, Máximo: {rttMax} ms, Máximo: {rttMax} ms";
                    }
                    else
                    {
                        lbl.Text = "Equipamento não responde: DOWN";
                    }
                }

                //lbl.Text = $"Packets transmitted: {packetsTransmitted} Received: {received} Packet loss: {packetLoss} Time (ms): {timeMs}";
            }
            else
            {
                lbl.Text = $"Trecho não encontrado na string. {ip}";
            }

            //lbl.Text = result;
        }

        private void CloseConection(SshClient client)
        {
            client.Disconnect();
        }

        private void StatusLoader(ActivityIndicator ldr, Button btn, string txt, bool sts)
        {
            btn.Text = txt;
            ldr.IsVisible = sts;
            ldr.IsRunning = sts;
        }
    }
}
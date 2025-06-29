using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tetriz
{
    class PecaTetris
    {
        public int[,] Blocos;
        public string Formato;

        public PecaTetris(string formato)
        {
            Formato = formato;
            Blocos = CriarMatrizDoFormato(formato);
        }

        private int[,] CriarMatrizDoFormato(string formato)
        {
            if (formato == "I") return new int[,] { { 1, 1, 1 }, { 0, 1, 0 }, { 0, 0, 0 } };
            if (formato == "L") return new int[,] { { 1, 0, 0 }, { 1, 1, 1 }, { 0, 0, 0 } };
            return new int[,] { { 0, 1, 0 }, { 1, 1, 1 }, { 0, 0, 0 } };
        }

        public void GirarHorario()
        {
            int[,] nova = new int[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    nova[j, 2 - i] = Blocos[i, j];
            Blocos = nova;
        }

        public void GirarAntiHorario()
        {
            int[,] nova = new int[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    nova[2 - j, i] = Blocos[i, j];
            Blocos = nova;
        }
    }

    class Jogador
    {
        public string Nome;
        public int Pontuacao;
        public Jogador(string nome) { Nome = nome; Pontuacao = 0; }
    }

    class JogoTetris
    {
        static int[,] grade = new int[20, 10];

        static void Main()
        {
            Console.Write("Digite seu nome: ");
            Jogador jogador = new Jogador(Console.ReadLine());

            Random sorteador = new Random();
            bool jogoAtivo = true;

            while (jogoAtivo)
            {
                string[] formatos = { "I", "L", "T" };
                PecaTetris peca = new PecaTetris(formatos[sorteador.Next(formatos.Length)]);

                int linha = 0, coluna = 4;
                bool pecaCaiu = false;

                while (!pecaCaiu)
                {
                    Console.Clear();
                    MostrarGradeComPeca(peca, linha, coluna);
                    Console.WriteLine($"Jogador: {jogador.Nome}  |  Pontos: {jogador.Pontuacao}");
                    Console.WriteLine("A‑Esq  D‑Dir  S‑Descer  E‑Horário  Q‑Anti‑Horário");

                    char comando = char.ToUpper(Console.ReadKey(true).KeyChar);

                    if (comando == 'A' && PodeMoverPara(peca, linha, coluna - 1))
                        coluna--;
                    else if (comando == 'D' && PodeMoverPara(peca, linha, coluna + 1))
                        coluna++;
                    else if (comando == 'S')
                    {
                        if (PodeMoverPara(peca, linha + 1, coluna))
                            linha++;
                        else
                            pecaCaiu = true;
                    }
                    else if (comando == 'E')
                    {
                        peca.GirarHorario();
                        if (!PodeMoverPara(peca, linha, coluna))
                            peca.GirarAntiHorario();
                    }
                    else if (comando == 'Q')
                    {
                        peca.GirarAntiHorario();
                        if (!PodeMoverPara(peca, linha, coluna))
                            peca.GirarHorario();
                    }
                }

                FixarPecaNaGrade(peca, linha, coluna);

                int linhasRemovidas = RemoverLinhasCompletas();
                if (linhasRemovidas > 0)
                    jogador.Pontuacao += 300 + (linhasRemovidas - 1) * 100;

                jogador.Pontuacao += peca.Formato == "I" ? 3 : peca.Formato == "L" ? 4 : 5;

                if (!ExisteEspacoParaNovaPeca())
                {
                    Console.Clear();
                    Console.WriteLine($"FIM DE JOGO!  Pontuação: {jogador.Pontuacao}");

                    try
                    {
                        StreamWriter arq = new StreamWriter("scores.txt", true, Encoding.UTF8);
                        arq.WriteLine($"{jogador.Nome};{jogador.Pontuacao}");
                        arq.Close();
                        Console.WriteLine("Pontuação salva em scores.txt");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Erro ao salvar arquivo: " + e.Message);
                    }

                    jogoAtivo = false;
                }
            }
        }

        static void MostrarGradeComPeca(PecaTetris p, int l0, int c0)
        {
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (ColideComPeca(p, i, j, l0, c0))
                        Console.Write("X ");
                    else if (grade[i, j] == 1)
                        Console.Write("O ");
                    else
                        Console.Write(". ");
                }
                Console.WriteLine();
            }
        }

        static bool ColideComPeca(PecaTetris p, int x, int y, int l0, int c0)
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (p.Blocos[i, j] == 1 && x == l0 + i && y == c0 + j)
                        return true;
            return false;
        }

        static bool PodeMoverPara(PecaTetris p, int novaL, int novaC)
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (p.Blocos[i, j] == 1)
                    {
                        int x = novaL + i, y = novaC + j;
                        if (x < 0 || x >= 20 || y < 0 || y >= 10 || grade[x, y] == 1)
                            return false;
                    }
            return true;
        }

        static void FixarPecaNaGrade(PecaTetris p, int l, int c)
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (p.Blocos[i, j] == 1)
                    {
                        int x = l + i, y = c + j;
                        if (x >= 0 && x < 20 && y >= 0 && y < 10)
                            grade[x, y] = 1;
                    }
        }

        static int RemoverLinhasCompletas()
        {
            int removidas = 0;
            for (int i = 0; i < 20; i++)
            {
                bool completa = true;
                for (int j = 0; j < 10; j++)
                    if (grade[i, j] == 0) { completa = false; break; }

                if (completa)
                {
                    removidas++;
                    for (int k = i; k > 0; k--)
                        for (int j = 0; j < 10; j++)
                            grade[k, j] = grade[k - 1, j];
                    for (int j = 0; j < 10; j++)
                        grade[0, j] = 0;
                }
            }
            return removidas;
        }

        static bool ExisteEspacoParaNovaPeca()
        {
            for (int j = 0; j < 10; j++)
                if (grade[0, j] == 1) return false;
            return true;
        }
    }
}

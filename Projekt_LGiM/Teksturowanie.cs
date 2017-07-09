﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Drawing = System.Drawing;

namespace Projekt_LGiM
{
    class Teksturowanie
    {
        private string sciezka;
        private Rysownik rysownik;
        private Drawing.Size rozmiarTekstury;
        private byte[] teksturaPixs;

        public Teksturowanie(string sciezka, Rysownik rysownik)
        {
            this.sciezka = sciezka;
            this.rysownik = rysownik;

            rozmiarTekstury = new Drawing.Bitmap(Drawing.Image.FromFile(sciezka)).Size;
            teksturaPixs = Rysownik.ToByteArray(sciezka);          
        }

        public void Teksturuj(List<Point> sciana, List<Point> tekstura)
        {
            for(int i = 0; i < tekstura.Count; ++i)
            {
                tekstura[i] = new Point(tekstura[i].X * rozmiarTekstury.Width, tekstura[i].Y * rozmiarTekstury.Height);
            }

            int startY = (int)Math.Min(Math.Min(sciana[0].Y, sciana[1].Y), sciana[2].Y);
            int endY   = (int)Math.Max(Math.Max(sciana[0].Y, sciana[1].Y), sciana[2].Y);

            var punktyWypelnienie = new List<int>();

            // Przechodź po obszarze figury od góry
            for (int y = startY; y < endY; ++y)
            {
                punktyWypelnienie.Clear();

                // Przechodź po krawędziach figury
                for (int i = 0; i < sciana.Count; ++i)
                {
                    int j = (i + 1) % sciana.Count;
                    int maxX = (int)Math.Max(sciana[i].X, sciana[j].X);
                    int minX = (int)Math.Min(sciana[i].X, sciana[j].X);
                    int maxY = (int)Math.Max(sciana[i].Y, sciana[j].Y);
                    int minY = (int)Math.Min(sciana[i].Y, sciana[j].Y);

                    int x = (int)Math.Floor((sciana[i].X - sciana[j].X) / (sciana[i].Y - sciana[j].Y) * (y - sciana[i].Y) + sciana[i].X);

                    // Sprawdź, czy punkt znajduje się na linii
                    if (x >= minX && x <= maxX && y >= minY && y <= maxY)
                    {
                        punktyWypelnienie.Add(x);
                    }
                }

                // Posortuj punkty w poziomie
                punktyWypelnienie.Sort();

                // Usuń powtarzające się punkty
                for (int i = 0; i < punktyWypelnienie.Count - 1; ++i)
                {
                    if (punktyWypelnienie[i] == punktyWypelnienie[i + 1])
                    {
                        punktyWypelnienie.RemoveAt(i);
                    }
                }

                // Dla obliczonych par punktów przechodź w poziomie
                if (punktyWypelnienie.Count > 1)
                {
                    for (int x = punktyWypelnienie[0]; x <= punktyWypelnienie[1]; ++x)
                    {
                        double mianownik = (sciana[1].X - sciana[0].X) * (sciana[2].Y - sciana[0].Y)
                                         - ((sciana[1].Y - sciana[0].Y) * (sciana[2].X - sciana[0].X));

                        double v = ((x - sciana[0].X) * (sciana[2].Y - sciana[0].Y)
                                 - ((y - sciana[0].Y) * (sciana[2].X - sciana[0].X))) / mianownik;

                        double w = ((sciana[1].X - sciana[0].X) * (y - sciana[0].Y)
                                 - ((sciana[1].Y - sciana[0].Y) * (x - sciana[0].X))) / mianownik;

                        double u = 1 - v - w;

                        // Jeśli punkt znajduje się w trójkącie to oblicz współrzędne trójkąta z tekstury
                        if (u >= 0 && u <= 1 && v >= 0 && v <= 1 && w >= 0 && w <= 1)
                        {
                            double tx = u * tekstura[0].X + v * tekstura[1].X + w * tekstura[2].X;
                            double ty = u * tekstura[0].Y + v * tekstura[1].Y + w * tekstura[2].Y;

                            double a = tx - Math.Floor(tx);
                            double b = ty - Math.Floor(ty);

                            if (Math.Floor(tx + 1) < rozmiarTekstury.Width && Math.Floor(ty + 1) < rozmiarTekstury.Height)
                            {
                                var kolorP1 = Rysownik.SprawdzKolor((int)Math.Floor(tx), (int)Math.Floor(ty), teksturaPixs,
                                    rozmiarTekstury.Width, rozmiarTekstury.Height);

                                var kolorP2 = Rysownik.SprawdzKolor((int)Math.Floor(tx), (int)Math.Floor(ty + 1),
                                    teksturaPixs, rozmiarTekstury.Width, rozmiarTekstury.Height);

                                var kolorP3 = Rysownik.SprawdzKolor((int)Math.Floor(tx + 1), (int)Math.Floor(ty), teksturaPixs,
                                    rozmiarTekstury.Width, rozmiarTekstury.Height);

                                var kolorP4 = Rysownik.SprawdzKolor((int)Math.Floor(tx + 1), (int)Math.Floor(ty + 1), teksturaPixs,
                                    rozmiarTekstury.Width, rozmiarTekstury.Height);

                                var kolor = new Color()
                                {
                                    // Oblicz kolor piksela
                                    R = (byte)((1 - b) * ((1 - a) * kolorP1.R + a * kolorP3.R) + b * ((1 - a) * kolorP2.R + a * kolorP4.R)),
                                    G = (byte)((1 - b) * ((1 - a) * kolorP1.G + a * kolorP3.G) + b * ((1 - a) * kolorP2.G + a * kolorP4.G)),
                                    B = (byte)((1 - b) * ((1 - a) * kolorP1.B + a * kolorP3.B) + b * ((1 - a) * kolorP2.B + a * kolorP4.B)),
                                    A = (byte)((1 - b) * ((1 - a) * kolorP1.A + a * kolorP3.A) + b * ((1 - a) * kolorP2.A + a * kolorP4.A))
                                };
                                rysownik.RysujPiksel(x, y, kolor.R, kolor.G, kolor.B, kolor.A);
                            }
                        }
                    } 
                }
            }
        }
    }
}

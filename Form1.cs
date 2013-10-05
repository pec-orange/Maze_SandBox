﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Maze_SandBox
{
    public partial class Form1 : Form
    {
        System.Drawing.Graphics g;
        int lenght_x = 15;
        int lenght_y = 15;
        int size = 35;
        cell[,] maze;
        Point current_position;
        Point exit_location;
        int[] all_steps = { 0, 1, 2, 3, 0, 1, 2, 3 }; // cycled steps sequence
        Point[] step_maker = {                      
                                new Point(0, -1),   // coordinates diviations
                                new Point(-1, 0),   // used to calculate coordinates of the next cell
                                new Point(0, 1),    // depending on which way you go
                                new Point(1, 0)     // 0 - up; 1 - left
                             };                     // 2 - down; 3 - right

        enum step_phase { Start, Walk, Hunt };


        public Form1()
        {
            InitializeComponent();
        }

        static Color SetTransparency(int A, Color source)
        {
            return Color.FromArgb(A, source.R, source.G, source.B);
        }

        private void draw_cell(int i, int j)
        {
            if (j < 0)
                j = lenght_y - 1;
            if (i < 0)
                i = lenght_x - 1;
            g = this.CreateGraphics();
            int x = size * i + i*2 + 2;
            int y = size * j + j*2 + 2;
            Pen pencil;
            SolidBrush filler;

            // --- background drawing
            filler = new SolidBrush(Color.White);
            g.FillRectangle(filler, x, y, size, size);

            // --- Walls drawing
            filler = new SolidBrush(Color.Black);
            if (maze[i, j].get_side(0) == 1)
                g.FillRectangle(filler, x - 2, y - 2, size + 3, 1);
            if (maze[i, j].get_side(1) == 1)
                g.FillRectangle(filler, x - 2, y - 2, 1, size + 3);
            if (maze[i, j].get_side(2) == 1)
                g.FillRectangle(filler, x - 2, y + size, size  + 3, 1);
            if (maze[i, j].get_side(3) == 1)
                g.FillRectangle(filler, x + size, y - 2, 1, size + 3);


            // --- specific marker drawing
            switch (maze[i, j].get_rank())
            {
                case 0:
                    break;
                case 1: // // Growing Tree/Walking Man in-stack cell marker
                    filler = new SolidBrush(SetTransparency(64, Color.Tomato));
                    g.FillRectangle(filler, x + size / 2 - size / 4, y + size / 2 - size / 4, size / 2, size / 2);
                    break;
                case 2: // starting cell
                    pencil = new Pen(Color.Red);
                    g.DrawRectangle(pencil, x + size / 2 - size / 4, y + size / 2 - size / 4, size / 2, size / 2);
                    break;
                case 3: // exit cell
                    pencil = new Pen(Color.Orange);
                    g.DrawRectangle(pencil, x + size / 2 - size / 4, y + size / 2 - size / 4, size / 2, size / 2);
                    break;
                case 4: // wave visited cell
                    pencil = new Pen(Color.YellowGreen);
                    g.DrawRectangle(pencil, x + size / 2 - size / 10, y + size / 2 - size / 10, size / 5, size / 5);
                    break;
                case 5: // winner wave cell
                    filler = new SolidBrush(SetTransparency(64, Color.Red));
                    g.FillRectangle(filler, x + size / 2 - size / 10, y + size / 2 - size / 10, size / 5, size / 5);
                    break;
                case 6: // Tremo visited cell
                    pencil = new Pen(Color.Blue);
                    g.DrawRectangle(pencil, x + size / 2 - size / 10, y + size / 2 - size / 10, size / 5, size / 5);
                    break;
                case 7: // Tremo/Hunt & Kill current cell marker
                    filler = new SolidBrush(SetTransparency(64, Color.Green));
                    g.FillRectangle(filler, x + size / 2 - size / 4, y + size / 2 - size / 4, size / 2, size / 2);
                    break;
                case 8: // Hunt & Kill not-finished cell marker
                    pencil = new Pen(Color.SkyBlue);
                    g.DrawRectangle(pencil, x + size / 2 - size / 10, y + size / 2 - size / 10, size / 5, size / 5);
                    break;
            }
        }

        private void draw_cell(Point position)
        {
            draw_cell(position.X, position.Y);
        }

        private void draw_line(int j)
        {
            g = this.CreateGraphics();
            SolidBrush filler = new SolidBrush(this.BackColor);
            // clear whole line
            g.FillRectangle(filler, 0, (size + 2) * j + 2, lenght_x * (size + 2) + 2, size);
            // redraw all cells one by one
            for (int i = 0; i < lenght_x; i++)
                draw_cell(i, j);
        }

        private void draw()
        {
            g = this.CreateGraphics();
            SolidBrush filler = new SolidBrush(this.BackColor);
            g.FillRectangle(filler, 0, 0, lenght_x * (size + 2) + 2, lenght_y * (size + 2) + 2);
            for (int i = 0; i < lenght_x; i++)
                for (int j = 0; j < lenght_y; j++)
                    draw_cell(i, j);
        }

        private void maze_update()
        {
            for (int j = 0; j < lenght_y; j++)
                for (int i = 0; i < lenght_x; i++)
                {
                    maze[i, j].set_rank(0);
                    if (i < lenght_x - 1)
                        if (maze[i + 1, j].get_side(1) == 1)
                            maze[i, j].set_side(3);
                    if (i > 0)
                        if (maze[i - 1, j].get_side(3) == 1)
                            maze[i, j].set_side(1);
                    if (j < lenght_y - 1)
                        if (maze[i, j + 1].get_side(0) == 1)
                            maze[i, j].set_side(2);
                    if (j > 0)
                        if (maze[i, j - 1].get_side(2) == 1)
                            maze[i, j].set_side(0);
                }
            maze[0, 0].set_rank(2);
            maze[exit_location.X, exit_location.Y].set_rank(3);
            draw();
        }

        private void maze_init()
        {
            if (g != null)
                g.Clear(this.BackColor);
            maze = new cell[lenght_x, lenght_y];

            for (int i = 0; i < lenght_x; i++)
                for (int j = 0; j < lenght_y; j++)
                {
                    maze[i, j] = new cell();
                    if (j == 0)
                        maze[i, j].set_side(0);
                    if (i == 0)
                        maze[i, j].set_side(1);
                    if (j == lenght_y - 1)
                        maze[i, j].set_side(2);
                    if (i == lenght_x - 1)
                        maze[i, j].set_side(3);
                }
            draw();
        }

        private void parameters_init()
        {
            size = Convert.ToInt32(cellSizeBox.Text);
            lenght_x = (this.Width - 220 - 10) / (size+2);
            lenght_y = (this.Height - 60) / (size+2);
            if ((exit_location.X == 0) && (exit_location.Y == 0))
            {
                exit_location.X = lenght_x - 1;
                exit_location.Y = lenght_y - 1;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            solveBox.SelectedIndex = 0;
            generateBox.SelectedIndex = 0;
            //parameters_init();
            textBox_TextChanged(sender, e);
            reset_button_Click(sender, e);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            controlPanel.Location = new Point(this.Width - 220, 5);
            textBox_TextChanged(sender, e);
            reset_button_Click(sender, e);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (selectCheckBox.Checked)
            {
                if ((exit_location.X >= lenght_x) || (exit_location.Y >= lenght_y))
                    exit_location.X = exit_location.Y = 0;
                maze[exit_location.X, exit_location.Y].set_rank(0);
                draw_cell(exit_location.X, exit_location.Y);
                exit_location.X = e.X / size;
                exit_location.Y = e.Y / size;
                if ((exit_location.X < lenght_x) && (exit_location.Y < lenght_y))
                {
                    maze[exit_location.X, exit_location.Y].set_rank(3);
                    draw_cell(exit_location.X, exit_location.Y);
                    statusLabel.Text = "New exit position: X = " + exit_location.X + ", Y = " + exit_location.Y;
                }
                else
                {
                    exit_location.X = 0;
                    exit_location.Y = 0;
                }

            }
        }

        private void Form1_MouseEnter(object sender, EventArgs e)
        {
            if (selectCheckBox.Checked)
                this.Cursor = Cursors.Cross;
        }

        private void Form1_MouseLeave(object sender, EventArgs e)
        {
            if (selectCheckBox.Checked)
                this.Cursor = Cursors.Default;
        }

        private void reset_button_Click(object sender, EventArgs e)
        {
            if (g != null)
                g.Clear(this.BackColor);
            current_position = new Point();
            parameters_init();
            if (speedCheckBox.Checked)
                maze_init();
            statusLabel.Text = "Layout reseted.";
        }

        private void generate_button_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            current_position.X = rnd.Next(0, lenght_x); // random starting point
            current_position.Y = rnd.Next(0, lenght_y); // for maze generation
            parameters_init();
            maze_init();
            int delay_gen = Convert.ToInt32(delayBox2.Text);
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            switch (generateBox.SelectedItem.ToString())
            {
                case "Hunt and Kill algorithm":
                    Hunt_and_Kill(delay_gen);
                    break;
                case "Walking Man algorithm":
                    Walking_Man(delay_gen);
                    break;
                case "Growing Tree algorithm":
                    Growing_Tree(delay_gen);
                    break;
            }
            timer.Stop();
            double disp = timer.ElapsedMilliseconds / 1000F;
            if (!animationCheckBox.Checked)
                draw();

            statusLabel.Text = generateBox.SelectedItem.ToString();
            if (optionsBox.SelectedItem != null)
                statusLabel.Text += " <" + optionsBox.SelectedItem.ToString() + ">";
            statusLabel.Text += ": time elapsed " + disp + " seconds.";

        }

        private void generateBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            optionsBox.Items.Clear();
            switch (generateBox.SelectedItem.ToString())
            {
                case "Hunt and Kill algorithm":
                    break;
                case "Walking Man algorithm":
                    optionsBox.Items.Add("Newest");
                    optionsBox.Items.Add("Random");
                    //optionsBox.Items.Add("Newest/Random, 50/50 split");
                    optionsBox.SelectedIndex = 0;
                    break;
                case "Growing Tree algorithm":
                    optionsBox.Items.Add("Newest (Backtracker)");
                    optionsBox.Items.Add("Random (Prim's)");
                    optionsBox.Items.Add("One of the newest");
                    optionsBox.Items.Add("Newest/Random, 75/25 split");
                    optionsBox.Items.Add("Newest/Random, 50/50 split");
                    optionsBox.Items.Add("Newest/Random, 25/75 split");
                    optionsBox.Items.Add("Middle");
                    optionsBox.Items.Add("Oldest");
                    optionsBox.SelectedIndex = 0;
                    break;

            }
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            parameters_init();
            mazeSizeBox.Text = lenght_x + "x" + lenght_y + " = " + lenght_x * lenght_y;
        }

        private void selectCheckBox_Click(object sender, EventArgs e)
        {
            if (selectCheckBox.Checked)
                selectCheckBox.BackColor = Color.LightBlue;
            else
                selectCheckBox.BackColor = solve_button.BackColor; // didnt find this color :(
        }

        private void Hunt_and_Kill(int delay = 0)
        {
            Random rnd = new Random();
            int step = 0;
            Point ghost_step = new Point(); // used to "ghost" to the next cell and analyse it
            bool dead_end = false;
            int sum = 0;
            bool step_done = false;
            bool gen_complete = false;
            byte[] bit_counter = new byte[4]; // used to store information about blicked directions
            // "1" - direction is blocked; "0" - direction is free to go
            int ban_way = 8; // first run - no ban way
            g = this.CreateGraphics();
            SolidBrush paint;
            int extra_lines = 0;
            bool found = false;
            while (!gen_complete)
            {
                // --- WALK
                dead_end = false;
                while (!dead_end)
                {
                    System.Threading.Thread.Sleep(delay);
                    maze[current_position.X, current_position.Y].visited = true;
                    maze[current_position.X, current_position.Y].set_rank(8);
                    
                    bit_counter = new byte[4];
                    sum = 0;
                    for (int t = 0; t < 4; t++)
                    {
                        ghost_step.X = current_position.X + step_maker[t].X;
                        ghost_step.Y = current_position.Y + step_maker[t].Y;
                        // set up walls to every nearby visited cells, except for the way back
                        if ((ghost_step.X >= 0) && (ghost_step.X < lenght_x) && (ghost_step.Y >= 0) && (ghost_step.Y < lenght_y))
                        {
                            if (maze[ghost_step.X, ghost_step.Y].visited)
                            {
                                if (ban_way != t)
                                    maze[current_position.X, current_position.Y].set_side(t);
                                bit_counter[t] = 1;
                            }
                        }
                        else
                            bit_counter[t] = 1;
                        sum += bit_counter[t];
                    }

                    while (ban_way == 9) // returning from "hunt" phase
                    {
                        step = rnd.Next(0, 4);
                        ghost_step.X = current_position.X + step_maker[step].X;
                        ghost_step.Y = current_position.Y + step_maker[step].Y;
                        if ((ghost_step.X >= 0) && (ghost_step.X < lenght_x) && (ghost_step.Y >= 0) && (ghost_step.Y < lenght_y) &&
                            maze[ghost_step.X, ghost_step.Y].visited)
                        {
                            maze[current_position.X, current_position.Y].set_side(step, 0);
                            break;
                        }
                    }

                    if (sum < 4)
                    {
                        step_done = false;
                        while (!step_done)
                        {
                            step = rnd.Next(0, 4);
                            if (bit_counter[step] != 1)
                            {
                                if (animationCheckBox.Checked)
                                    draw_cell(current_position);
                                current_position.X = current_position.X + step_maker[step].X;
                                current_position.Y = current_position.Y + step_maker[step].Y;
                                step_done = true;
                                ban_way = all_steps[step + 2];
                                maze[current_position.X, current_position.Y].set_rank(7);
                            }
                        }
                    }
                    else
                    {
                        dead_end = true;
                    }
                    
                    if (animationCheckBox.Checked)
                        draw_cell(current_position);
                }
                // --- HUNT
                found = false;
                for (int j = extra_lines; j < lenght_y; j++)
                {
                    System.Threading.Thread.Sleep(delay*2);
                    if (animationCheckBox.Checked)
                    {
                        if (j > 0)
                            draw_line(j - 1);
                        paint = new SolidBrush(SetTransparency(32, Color.LightGreen));
                        g.FillRectangle(paint, size / 4, (size + 2) * j + 2 + size / 8, lenght_x * (size + 2) - size / 2, 3 * size / 4);
                    }
                    for (int i = 0; i < lenght_x; i++)
                    {
                        if (!maze[i, j].visited)
                        {
                            current_position = new Point(i, j);
                            for (int t = 0; t < 4; t++)
                            {
                                ghost_step.X = current_position.X + step_maker[t].X;
                                ghost_step.Y = current_position.Y + step_maker[t].Y;
                                if ((ghost_step.X >= 0) && (ghost_step.X < lenght_x) && (ghost_step.Y >= 0) && (ghost_step.Y < lenght_y) &&
                                    maze[ghost_step.X, ghost_step.Y].visited)
                                {
                                    found = true;
                                    i = lenght_x;
                                    j = lenght_y; 
                                    break;
                                }
                            }
                        }
                        else if ((i == lenght_x - 1) && speedCheckBox.Checked)
                        {
                            maze[i, j].set_rank(0);
                            extra_lines++;
                        }
                        else
                            maze[i, j].set_rank(0);
                    }
                }
                if (found)
                {
                    ban_way = 9;
                    maze[current_position.X, current_position.Y].set_rank(7);
                    draw_cell(current_position);
                    System.Threading.Thread.Sleep(delay * 3);
                }
                else
                {
                    gen_complete = true;
                    if (animationCheckBox.Checked)
                        draw_line(lenght_y - 1);
                }
                if (animationCheckBox.Checked)
                    draw_line(current_position.Y);
            }
        }
        
        private void Walking_Man(int delay = 0)
        {
            Point[] Storage = new Point[lenght_x*lenght_y];
            int length = 0;
            Random rnd = new Random();
            int step = 0;
            Point ghost_step = new Point(); // used to "ghost" to the next cell and analyse it
            bool dead_end = false;
            int sum = 0;
            bool step_done = false;
            bool gen_complete = false;
            string generation_options = optionsBox.SelectedItem.ToString();
            byte[] bit_counter = new byte[4]; // used to store information about blicked directions
            // "1" - direction is blocked; "0" - direction is free to go
            
            int ban_way = 8; // first run - no ban way
            while (!gen_complete)
            {
                while (!dead_end)
                {
                    Storage[length++] = current_position;

                    // if this cell is already visited - it will be deleted from stack later on
                    // however in-stack marker is cleared right away!
                    if (maze[current_position.X, current_position.Y].visited)
                    {
                        maze[current_position.X, current_position.Y].set_rank(0);
                        length--;   // delete this cell from stack because its already visited
                    }
                    else
                    {
                        maze[current_position.X, current_position.Y].set_rank(1);
                        maze[current_position.X, current_position.Y].visited = true;
                    }
                    
                    System.Threading.Thread.Sleep(delay);
                        
                    sum = 0;
                    bit_counter = new byte[4];
                    for (int t = 0; t < 4; t++)
                    {
                        ghost_step.X = current_position.X + step_maker[t].X;
                        ghost_step.Y = current_position.Y + step_maker[t].Y;
                        //if ghosted cell is outside of the maze - block this way
                        if ((ghost_step.X < 0) || (ghost_step.X >= lenght_x) || (ghost_step.Y < 0) || (ghost_step.Y >= lenght_y))
                            bit_counter[t] = 1;
                        else
                        {
                            if (maze[ghost_step.X, ghost_step.Y].visited)
                            {
                                if ((ban_way != t)&&(ban_way != 9)) // ban_way = 9 if its the beginning of new walk iteration
                                    maze[current_position.X, current_position.Y].set_side(t);
                                bit_counter[t] = 1;
                            }
                        }
                        sum += bit_counter[t];
                    }
                    if (sum < 4)
                    {
                        step_done = false;
                        while (!step_done)
                        {
                            step = rnd.Next(0, 4);
                            if (bit_counter[step] != 1)
                            {
                                if (animationCheckBox.Checked)
                                    draw_cell(current_position);
                                current_position.X = current_position.X + step_maker[step].X;
                                current_position.Y = current_position.Y + step_maker[step].Y;
                                step_done = true;
                                ban_way = all_steps[step + 2];
                            }
                        }
                        if (animationCheckBox.Checked)
                            draw_cell(current_position);
                    }
                    else
                    {
                        dead_end = true;
                        maze[current_position.X, current_position.Y].set_rank(0);
                        if (animationCheckBox.Checked)
                            draw_cell(current_position);
                    }

                }
                if (length >  1)
                {
                    switch (generation_options)
                    {
                        case "Newest":
                            current_position = Storage[length - 2];
                            break;
                        case "Random":
                            sum = rnd.Next(length);
                            current_position = Storage[sum];
                            for (int r = sum; r < length - 2; r++)
                                Storage[r] = Storage[r + 1];
                                break;
                        case "Newest/Random, 50/50 split":  //  this is not working at this moment
                            if (rnd.Next(2) == 1)
                                current_position = Storage[length - 2];
                            else
                            {
                                sum = rnd.Next(length);
                                current_position = Storage[sum];
                                for (int r = sum; r < length - 2; r++)
                                    Storage[r] = Storage[r + 1];
                            }
                            break;
                    }
                    length--;
                    dead_end = false;
                    ban_way = 9; // triggers unstuck mechanism
                }
                else
                    gen_complete = true;
            }
            if (length > 0) // clean up call
                for (int r = 0; r < length; r++)
                {
                    maze[Storage[r].X, Storage[r].Y].set_rank(0);
                    if (animationCheckBox.Checked)
                        draw_cell(Storage[r]);
                }
        }

        private void Growing_Tree(int delay = 0)
        {
            Point[] Storage = new Point[lenght_x * lenght_y];
            int length = 0;
            Random rnd = new Random();
            int step = 0;
            Point ghost_step = new Point(); // used to "ghost" to the next cell and analyse it
            bool dead_end = false;
            int sum = 0;
            bool step_done = false;
            bool gen_complete = false;
            string generation_options = optionsBox.SelectedItem.ToString();
            byte[] bit_counter = new byte[4]; // used to store information about blicked directions
            // "1" - direction is blocked; "0" - direction is free to go

            int ban_way = 8; // first run - no ban way

            while (!dead_end)
            {
                maze[current_position.X, current_position.Y].set_rank(1);
                maze[current_position.X, current_position.Y].visited = true;

                Storage[length++] = current_position;

                System.Threading.Thread.Sleep(delay);

                sum = 0;
                bit_counter = new byte[4];
                for (int t = 0; t < 4; t++)
                {
                    ghost_step.X = current_position.X + step_maker[t].X;
                    ghost_step.Y = current_position.Y + step_maker[t].Y;
                    if ((ghost_step.X < 0) || (ghost_step.X >= lenght_x) || (ghost_step.Y < 0) || (ghost_step.Y >= lenght_y))
                        bit_counter[t] = 1;
                    else
                    {
                        if (maze[ghost_step.X, ghost_step.Y].visited)
                        {
                            if ((ban_way != t))
                                maze[current_position.X, current_position.Y].set_side(t);
                            bit_counter[t] = 1;
                        }
                    }
                    sum += bit_counter[t];
                }

                if (sum < 4)
                {
                    step_done = false;
                    while (!step_done)
                    {
                        step = rnd.Next(0, 4);
                        if (bit_counter[step] != 1)
                        {
                            if (animationCheckBox.Checked)
                                draw_cell(current_position);
                            current_position.X = current_position.X + step_maker[step].X;
                            current_position.Y = current_position.Y + step_maker[step].Y;
                            step_done = true;
                            ban_way = all_steps[step + 2];
                        }
                    }
                    if (animationCheckBox.Checked)
                        draw_cell(current_position);
                }
                else
                {
                    dead_end = true;
                    maze[current_position.X, current_position.Y].set_rank(0);
                    if (animationCheckBox.Checked)
                        draw_cell(current_position);
                }
            }

            while (!gen_complete)
            {
                if (length > 0)
                {
                    switch (generation_options)
                    {
                        case "Newest (Backtracker)":
                            current_position = Storage[length - 1];
                            break;
                        case "Random (Prim's)":
                            sum = rnd.Next(length);
                            current_position = Storage[sum];
                            for (int r = sum; r < length - 1; r++)
                                Storage[r] = Storage[r + 1];
                            break;
                        case "One of the newest":
                            if (length > 5)
                                sum = rnd.Next(5);
                            else
                                sum = rnd.Next(length);
                            sum = length - sum - 1;
                            current_position = Storage[sum];
                            for (int r = sum; r < length - 1; r++)
                                Storage[r] = Storage[r + 1];
                            break;
                        case "Newest/Random, 75/25 split":
                            if (rnd.Next(4) != 0)
                                current_position = Storage[length - 1];
                            else
                            {
                                sum = rnd.Next(length);
                                current_position = Storage[sum];
                                for (int r = sum; r < length - 1; r++)
                                    Storage[r] = Storage[r + 1];
                            }
                            break;
                        case "Newest/Random, 50/50 split":
                            if (rnd.Next(2) == 0)
                                current_position = Storage[length - 1];
                            else
                            {
                                sum = rnd.Next(length);
                                current_position = Storage[sum];
                                for (int r = sum; r < length - 1; r++)
                                    Storage[r] = Storage[r + 1];
                            }
                            break;
                        case "Newest/Random, 25/75 split":
                            if (rnd.Next(4) == 0)
                                current_position = Storage[length - 1];
                            else
                            {
                                sum = rnd.Next(length);
                                current_position = Storage[sum];
                                for (int r = sum; r < length - 1; r++)
                                    Storage[r] = Storage[r + 1];
                            }
                            break;
                        case "Middle":
                            current_position = Storage[length / 2];

                            for (int r = length/2; r < length - 1; r++)
                                Storage[r] = Storage[r + 1];
                            break;
                        case "Oldest":
                            current_position = Storage[0];

                            for (int r = 0; r < length - 1; r++)
                                Storage[r] = Storage[r + 1];
                            break;
                    }
                    // phase two
                    Storage[length - 1] = current_position;
                    System.Threading.Thread.Sleep(delay);
                    sum = 0;
                    bit_counter = new byte[4];
                    for (int t = 0; t < 4; t++)
                    {
                        ghost_step.X = current_position.X + step_maker[t].X;
                        ghost_step.Y = current_position.Y + step_maker[t].Y;
                        if ((ghost_step.X < 0) || (ghost_step.X >= lenght_x) || (ghost_step.Y < 0) || (ghost_step.Y >= lenght_y))
                            bit_counter[t] = 1;
                        else
                            if (maze[ghost_step.X, ghost_step.Y].visited)
                                bit_counter[t] = 1;
                        sum += bit_counter[t];
                    }
                    if (sum < 4)
                    {
                        step_done = false;
                        while (!step_done)
                        {
                            step = rnd.Next(0, 4);
                            if (bit_counter[step] != 1)
                            {
                                if (animationCheckBox.Checked)
                                    draw_cell(current_position);
                                current_position.X = current_position.X + step_maker[step].X;
                                current_position.Y = current_position.Y + step_maker[step].Y;
                                step_done = true;
                                ban_way = all_steps[step + 2];
                            }
                        }
                        for (int t = 0; t < 4; t++)
                        {
                            ghost_step.X = current_position.X + step_maker[t].X;
                            ghost_step.Y = current_position.Y + step_maker[t].Y;
                            if ((ghost_step.X >= 0) && (ghost_step.X < lenght_x) && (ghost_step.Y >= 0) && (ghost_step.Y < lenght_y) &&
                                        maze[ghost_step.X, ghost_step.Y].visited && (ban_way != t))
                                maze[current_position.X, current_position.Y].set_side(t);
                        }
                        maze[current_position.X, current_position.Y].set_rank(1);
                        maze[current_position.X, current_position.Y].visited = true;
                        Storage[length++] = current_position;

                        sum = 0; // breaks 'if' chain, please let it live!   
                    }
                    else
                    {
                        maze[current_position.X, current_position.Y].set_rank(0);
                        length--;
                    }
                    if (animationCheckBox.Checked)
                        draw_cell(current_position);
                    sum = 0;    // breaks 'if' chain, please let it live!
                }
                else
                    gen_complete = true;

            }
                
            
        }
}


    class cell
    {
        int[] sides = new int[4]; // 0 - free side; 1 - wall side
        public bool visited = false;       // indicates if this cell was already visited
        int rank = 0;             // additional user info. Used ot mark specific cells
        public cell()
        {
        }
        public int get_rank()
        {
            return rank;
        }
        public void set_rank(int source)
        {
            rank = source;
        }
        public void set_side(int num, int free = 1)
        {
            sides[num] = free;
        }
        public int get_side(int num)
        {
            return sides[num];
        }
    }

    class wave
    {
        bool winner = false;
        int lenght = 0;
        int[] legacy_x = new int[0];
        int[] legacy_y = new int[0];
        public wave(int x, int y)
        {
            if ((x >= 0) && (y >= 0))
                add(x, y);
            else
                MessageBox.Show("Добаление отрицательного элемента в волну");
        }
        public wave(wave source, int position)
        {
            int len = source.get_lenght();
            if (len != 0)
            {
                int[] holder;
                legacy_x = new int[position + 1];
                legacy_y = new int[position + 1];
                lenght = position + 1;
                for (int i = 0; i <= position; i++)
                {
                    holder = source.get_element(i);
                    legacy_x[i] = holder[0];
                    legacy_y[i] = holder[1];
                }
            }
        }
        public int find_element(int x, int y)
        {
            for (int i = 0; i < lenght; i++)
            {
                if ((legacy_x[i] == x) && (legacy_y[i] == y))
                    return i;
            }
            return -1;
        }
        public int get_lenght()
        {
            return lenght;
        }
        public void set_winner()
        {
            winner = true;
        }
        public bool get_winner()
        {
            return winner;
        }
        public int[] get_element(int position)
        {
            int[] result = new int[2];
            result[0] = legacy_x[position];
            result[1] = legacy_y[position];
            return result;
        }
        public void add(int x, int y)
        {
            int[] temp = legacy_x;
            legacy_x = new int[temp.Length + 1];
            for (int i = 0; i < temp.Length; i++)
                legacy_x[i] = temp[i];
            legacy_x[temp.Length] = x;

            temp = legacy_y;
            legacy_y = new int[temp.Length + 1];
            for (int i = 0; i < temp.Length; i++)
                legacy_y[i] = temp[i];
            legacy_y[temp.Length] = y;

            lenght++;
        }
    }
}
using System;
using System.Windows.Forms;
using System.Drawing;

namespace AnoreksikSuite {
    public partial class MainForm {
        private Label CreateLabel(string t, int x, int y, Control p) { return CreateLabel(t, x, y, 9, p); }
        private Label CreateLabel(string t, int x, int y, int size, Control p) { Label l = new Label(); l.Text = t; l.Location = new Point(x, y); l.AutoSize = true; if(size > 0) l.Font = new Font("Arial", size); p.Controls.Add(l); return l; }
        private Button CreateBtn(string t, int x, int y, int w, Color c, Control p) { Button b = new Button(); b.Text=t; b.Location=new Point(x,y); b.Size=new Size(w,40); b.BackColor=c; b.ForeColor=Color.White; b.FlatStyle=FlatStyle.Flat; p.Controls.Add(b); return b; }
        private NumericUpDown CreateNum(decimal v, decimal min, decimal max, int x, int y, int w, Control p) {
            NumericUpDown n = new NumericUpDown();
            n.Minimum = min;
            n.Maximum = max;
            if (v < min) v = min;
            if (v > max) v = max;
            n.Value = v;
            n.Location = new Point(x, y);
            n.Size = new Size(w, 20);
            n.BackColor = Color.FromArgb(40, 40, 40);
            n.ForeColor = Color.White;
            p.Controls.Add(n);
            return n;
        }
        private CheckBox CreateCheck(string t, int x, int y, Control p) { CheckBox c = new CheckBox(); c.Text=t; c.Location=new Point(x,y); c.AutoSize=true; p.Controls.Add(c); return c; }
        private GroupBox CreateGroup(string t, int x, int y, int w, int h, Control p) { GroupBox g = new GroupBox(); g.Text=t; g.ForeColor=Color.White; g.Location=new Point(x,y); g.Size=new Size(w,h); p.Controls.Add(g); return g; }
    }
}

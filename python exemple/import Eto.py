import Eto.Forms as forms
import Eto.Drawing as drawing
import System
import Rhino.UI
import math

class CircleProperties:
    def __init__(self, icon_path=""):
        self.fill_color = drawing.Colors.White
        self.border_color = None  # Will be set dynamically
        self.highlight_color = drawing.Colors.Orange
        self.icon_path = icon_path  # Icon path attribute with a default value of ""

class TransparentForm(forms.Form):
    def __init__(self):
        self.Title = "PIE Menu 0.1"
        self.WindowStyle = forms.WindowStyle.None
        self.AutoSize = False
        self.Resizable = False
        self.TopMost = True
        self.ShowActivated = False
        self.Size = drawing.Size(300, 300)
        self.Padding = drawing.Padding(20)
        self.MovableByWindowBackground = False

        # Initialize circle properties list with icon paths
        icon_paths = [
        "user_set_path",
        "user_set_path",
        "user_set_path",
        "user_set_path",
        "user_set_path",
        "user_set_path",
        "user_set_path",
        "user_set_path",
        "user_set_path",
        "user_set_path",
        ]
        self.circle_properties = [CircleProperties(icon_path) for icon_path in icon_paths]

        self.form_bitmap = drawing.Bitmap(drawing.Size(300, 300), drawing.PixelFormat.Format32bppRgba)
        self.DrawCirclesOnBitmap(self.form_bitmap, None)

        self.image_view = forms.ImageView()
        self.image_view.Image = self.form_bitmap

        layout = forms.DynamicLayout()
        layout.AddRow(self.image_view)
        self.Content = layout

        self.selected_button_index = None  # Initialize selected button index variable

        # Add transparent style
        self.Styles.Add[forms.Panel]("transparent", self.MyFormStyler)
        self.Style = "transparent"
        
    def DrawCirclesOnBitmap(self, bitmap, pointed_circle_index):
        # Clear the bitmap
        graphics = drawing.Graphics(bitmap)
        graphics.Clear(drawing.Colors.Transparent)

        circle_radius = 100
        num_circles = 8
        button_radius = 35

        center_x = bitmap.Size.Width / 2
        center_y = bitmap.Size.Height / 2

        for i, properties in enumerate(self.circle_properties):
            angle = (2 * math.pi / num_circles) * i
            circle_center_x = center_x + circle_radius * math.cos(angle)
            circle_center_y = center_y + circle_radius * math.sin(angle)
            circle_position = drawing.Point(circle_center_x - button_radius, circle_center_y - button_radius)
            circle_size = drawing.Size(2 * button_radius, 2 * button_radius)
          
            circ_brush = drawing.SolidBrush(properties.fill_color)  # Use circle properties

            highlight_color = drawing.Colors.Orange  # Set the highlight color
            border_color = drawing.Colors.LightGrey  # Assign a unique border color for each circle
            
            if i == pointed_circle_index:
                border_pen = drawing.Pen(highlight_color, 10)
                self.selected_button_index = i  # Update selected button index
            else:
                border_pen = drawing.Pen(border_color, 5)

            graphics.FillEllipse(circ_brush, drawing.Rectangle(circle_position, circle_size))
            graphics.DrawEllipse(border_pen, drawing.Rectangle(circle_position, circle_size)) 
            # Draw the image
            icon_bitmap = drawing.Bitmap(properties.icon_path)
            graphics.DrawImage(icon_bitmap, circle_center_x - 25, circle_center_y - 25, 50, 50)

        graphics.Dispose()

    def MyFormStyler(self, control):
        self.BackgroundColor = drawing.Colors.Transparent
        window = control.ControlObject
        if hasattr(window, "AllowsTransparency"):
            window.AllowsTransparency = True
        if hasattr(window, "Background"):
            brush = window.Background.Clone()
            brush.Opacity = 0  # Adjust opacity as needed
            window.Background = brush
        else:
            color = window.BackgroundColor
            window.BackgroundColor = color.FromRgba(0, 0, 0, 0)

    def Cleanup(self):
        # Close the form if it's shown
        if self.Visible:
            self.Close()
        
        # Dispose of the form's resources
        self.Dispose()

class MyMouseCallback(Rhino.UI.MouseCallback):
    def __init__(self):
        self.original_position = None
        self.form = None
        self.mouse_moved = False

    def OnMouseDown(self, e):
        if e.Button == System.Windows.Forms.MouseButtons.Middle:
            Rhino.RhinoApp.WriteLine("MMB Down")
            if self.form:
                self.form.Cleanup()  # Clean up any existing UI
            self.original_position = Rhino.UI.MouseCursor.Location
            self.form = TransparentForm()
            form_loc = drawing.Point(self.original_position.X - 150, self.original_position.Y - 150)
            Rhino.RhinoApp.WriteLine(str(form_loc))
            self.form.Location = form_loc
            self.form.Owner = Rhino.UI.RhinoEtoApp.MainWindow
            self.form.Show()

    def OnMouseMove(self, e):
        if self.original_position:
            current_position = Rhino.UI.MouseCursor.Location
            
            if current_position != self.original_position:
                self.mouse_moved = True

                vector_x = current_position.X - self.original_position.X
                vector_y = current_position.Y - self.original_position.Y

                angle = math.atan2(vector_y, vector_x)
                angle_degrees = math.degrees(angle)
                
                if angle_degrees < 0:
                    angle_degrees += 360
                
                num_circles = 8
                pointed_circle_index = int(angle_degrees / (360 / num_circles))
                # Rhino.RhinoApp.WriteLine("Button Index: "+ str(pointed_circle_index) )

                # Create a new bitmap for highlighting
                highlight_bitmap = drawing.Bitmap(drawing.Size(300, 300), drawing.PixelFormat.Format32bppRgba)
                # Call the DrawCirclesOnBitmap method with the highlighted circle
                self.form.DrawCirclesOnBitmap(highlight_bitmap, pointed_circle_index)
                # Update the ImageView with the new highlight_bitmap
                self.form.image_view.Image = highlight_bitmap

    def OnMouseUp(self, e):
        if e.Button == System.Windows.Forms.MouseButtons.Middle:
            if self.mouse_moved:
                self.original_position = None
                if self.form:
                    selected_index = self.form.selected_button_index
                    # Close the form first
                    self.form.Close()
                    self.form = None
                    if selected_index is not None:
                        # Execute the corresponding Rhino command
                        commands = ["Floor", "Wall", "Door", "Window", "Electrical", "Plumbing", "Object", "Room"]
                        Rhino.RhinoApp.WriteLine("Running {} tool...".format(commands[selected_index]))
                        if selected_index < len(commands):
                            Rhino.RhinoApp.RunScript(commands[selected_index], False)
                        else:
                            Rhino.RhinoApp.WriteLine("No command found for index {}.".format(selected_index))
            elif self.form:
                # If mouse hasn't moved, close the form
                self.form.Close()
                self.form = None


def DoSomething():
    if scriptcontext.sticky.has_key('MyMouseCallback'):
        callback = scriptcontext.sticky['MyMouseCallback']
        if callback:
            callback.Enabled = False
            callback = None
            scriptcontext.sticky.Remove('MyMouseCallback')
    else:
        callback = MyMouseCallback()
        callback.Enabled = True
        scriptcontext.sticky['MyMouseCallback'] = callback
        Rhino.RhinoApp.WriteLine("Click somewhere...")

if __name__ == "__main__":
    DoSomething()
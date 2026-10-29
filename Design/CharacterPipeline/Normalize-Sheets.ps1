param([string]$ManifestPath = (Join-Path $PSScriptRoot 'generation-manifest.json'))
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;
public static class PackSheetLayout {
  public static string Normalize(string source, string destination, int rows, bool effect) {
    using (var input = new Bitmap(source)) {
      var pixels = input.LockBits(new Rectangle(0,0,input.Width,input.Height),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
      var data = new byte[Math.Abs(pixels.Stride)*input.Height];
      Marshal.Copy(pixels.Scan0,data,0,data.Length);
      int stride=pixels.Stride; input.UnlockBits(pixels);
      var bounds=new List<Rectangle>(); int transparent=0, solid=0, edgeCells=0;
      int[] labels=new int[input.Width*input.Height]; int[] selected=new int[rows*6];
      var componentBounds=new List<Rectangle>{Rectangle.Empty}; var componentSizes=new List<int>{0};
      for(int y=0;y<input.Height;y++) for(int x=0;x<input.Width;x++) { int a=data[y*stride+x*4+3]; if(a==0) transparent++; if(a>24) solid++; }
      if(transparent < input.Width*input.Height/10) throw new Exception("Missing transparent background: "+source);
      if(!effect) {
        var queue=new int[input.Width*input.Height]; int component=0;
        for(int sy=0;sy<input.Height;sy++) for(int sx=0;sx<input.Width;sx++) {
          int first=sy*input.Width+sx;
          if(labels[first]!=0 || data[sy*stride+sx*4+3]<=24) continue;
          component++; int head=0,tail=0;queue[tail++]=first; labels[first]=component;
          int left=sx,right=sx,top=sy,bottom=sy;
          while(head<tail) {
            int current=queue[head++],cx=current%input.Width,cy=current/input.Width;
            left=Math.Min(left,cx);right=Math.Max(right,cx);top=Math.Min(top,cy);bottom=Math.Max(bottom,cy);
            for(int ny=Math.Max(0,cy-1);ny<=Math.Min(input.Height-1,cy+1);ny++) for(int nx=Math.Max(0,cx-1);nx<=Math.Min(input.Width-1,cx+1);nx++) {
              int next=ny*input.Width+nx;
              if(labels[next]==0 && data[ny*stride+nx*4+3]>24) {labels[next]=component;queue[tail++]=next;}
            }
          }
          componentBounds.Add(new Rectangle(left,top,right-left+1,bottom-top+1));componentSizes.Add(tail);
        }
        int originalComponents=componentBounds.Count;
        for(int id=1;id<originalComponents;id++) {
          var region=componentBounds[id];
          if(region.Width<input.Width/6f*1.45f && region.Height<input.Height/(float)rows*1.45f)continue;
          var splitIds=new Dictionary<int,int>();
          for(int py=region.Top;py<region.Bottom;py++)for(int px=region.Left;px<region.Right;px++)if(labels[py*input.Width+px]==id) {
            int slot=Math.Min(rows-1,py*rows/input.Height)*6+Math.Min(5,px*6/input.Width),split;
            if(!splitIds.TryGetValue(slot,out split)){split=componentBounds.Count;splitIds[slot]=split;componentBounds.Add(new Rectangle(px,py,1,1));componentSizes.Add(0);}
            labels[py*input.Width+px]=split;componentSizes[split]++;
            var b=componentBounds[split];int l=Math.Min(b.Left,px),t=Math.Min(b.Top,py),r=Math.Max(b.Right,px+1),bt=Math.Max(b.Bottom,py+1);componentBounds[split]=new Rectangle(l,t,r-l,bt-t);
          }
          componentSizes[id]=0;
        }
        for(int id=1;id<componentBounds.Count;id++)if(componentSizes[id]>200){var b=componentBounds[id];int col=Math.Min(5,Math.Max(0,(int)((b.Left+b.Right)*.5*6/input.Width)));int row=Math.Min(rows-1,Math.Max(0,(int)((b.Top+b.Bottom)*.5*rows/input.Height)));int slot=row*6+col;if(componentSizes[selected[slot]]<componentSizes[id])selected[slot]=id;}
      }
      for(int row=0;row<rows;row++) for(int col=0;col<6;col++) {
        int x0=(int)Math.Round(col*input.Width/6.0),x1=(int)Math.Round((col+1)*input.Width/6.0);
        int y0=(int)Math.Round(row*input.Height/(double)rows),y1=(int)Math.Round((row+1)*input.Height/(double)rows);
        int minX=x1,maxX=x0-1,minY=y1,maxY=y0-1;
        if(effect) { for(int y=y0;y<y1;y++) for(int x=x0;x<x1;x++) if(data[y*stride+x*4+3]>24) { minX=Math.Min(minX,x);maxX=Math.Max(maxX,x);minY=Math.Min(minY,y);maxY=Math.Max(maxY,y); } }
        else {var region=componentBounds[selected[row*6+col]];minX=region.Left;maxX=region.Right-1;minY=region.Top;maxY=region.Bottom-1;}
        if(maxX<minX) throw new Exception("Empty frame row="+row+" col="+col+": "+source);
        if(minX<=x0+1 || maxX>=x1-2 || minY<=y0+1 || maxY>=y1-2) edgeCells++;
        bounds.Add(new Rectangle(minX-x0,minY-y0,maxX-minX+1,maxY-minY+1));
      }
      float cellW=input.Width/6f,cellH=input.Height/(float)rows;
      float height=bounds.Take(6).Select(b=>b.Height).OrderBy(v=>v).ElementAt(3);
      float baseline=bounds.Take(6).Select(b=>b.Bottom).OrderBy(v=>v).ElementAt(3);
      float scale=effect?.82f*Math.Min(256f/cellW,256f/cellH):160f/height;
      int clipped=0;
      using(var output=new Bitmap(1536,rows*256,PixelFormat.Format32bppArgb)) {
        using(var graphics=Graphics.FromImage(output)) {
          graphics.Clear(Color.Transparent); graphics.CompositingMode=CompositingMode.SourceCopy;
          graphics.InterpolationMode=InterpolationMode.HighQualityBicubic; graphics.PixelOffsetMode=PixelOffsetMode.HighQuality;
          using(var attributes=new ImageAttributes()) {
            attributes.SetWrapMode(WrapMode.TileFlipXY);
            for(int row=0;row<rows;row++) for(int col=0;col<6;col++) {
              int x0=(int)Math.Round(col*input.Width/6.0),x1=(int)Math.Round((col+1)*input.Width/6.0);
              int y0=(int)Math.Round(row*input.Height/(double)rows),y1=(int)Math.Round((row+1)*input.Height/(double)rows);
              float rowBaseline=bounds.Skip(row*6).Take(6).Select(v=>v.Bottom).OrderBy(v=>v).ElementAt(3);
              float dx=128-cellW*.5f*scale,dy=effect?128-cellH*.5f*scale:208-rowBaseline*scale;
              var b=bounds[row*6+col];
              if(dx+b.Left*scale<0 || dx+b.Right*scale>256 || dy+b.Top*scale<0 || dy+b.Bottom*scale>256) {clipped++;Console.WriteLine("Clipped candidate "+Path.GetFileName(source)+" row="+row+" col="+col+" bounds="+b+" scale="+scale+" dx="+dx+" dy="+dy);}
              var state=graphics.Save(); graphics.SetClip(new Rectangle(col*256,row*256,256,256));
              if(effect) graphics.DrawImage(input,new RectangleF(col*256+dx,row*256+dy,(x1-x0)*scale,(y1-y0)*scale),new RectangleF(x0,y0,x1-x0,y1-y0),GraphicsUnit.Pixel);
              else {
                int label=selected[row*6+col];var region=componentBounds[label];
                int left=Math.Max(0,region.Left-2),top=Math.Max(0,region.Top-2),right=Math.Min(input.Width,region.Right+2),bottom=Math.Min(input.Height,region.Bottom+2);
                using(var actor=new Bitmap(right-left,bottom-top,PixelFormat.Format32bppArgb)) {
                  for(int py=top;py<bottom;py++) for(int px=left;px<right;px++) {
                    bool keep=labels[py*input.Width+px]==label;
                    if(!keep && data[py*stride+px*4+3]<=24) for(int yy=Math.Max(0,py-1);yy<=Math.Min(input.Height-1,py+1);yy++) for(int xx=Math.Max(0,px-1);xx<=Math.Min(input.Width-1,px+1);xx++) if(labels[yy*input.Width+xx]==label)keep=true;
                    if(keep)actor.SetPixel(px-left,py-top,Color.FromArgb(data[py*stride+px*4+3],data[py*stride+px*4+2],data[py*stride+px*4+1],data[py*stride+px*4]));
                  }
                  graphics.DrawImage(actor,new RectangleF(col*256+dx+(left-x0)*scale,row*256+dy+(top-y0)*scale,actor.Width*scale,actor.Height*scale),new RectangleF(0,0,actor.Width,actor.Height),GraphicsUnit.Pixel);
                }
              }
              graphics.Restore(state);
            }
          }
        }
        if(clipped>0) throw new Exception("Normalization would clip "+clipped+" frames: "+source);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)); output.Save(destination,ImageFormat.Png);
      }
      return "{\"sourceWidth\":"+input.Width+",\"sourceHeight\":"+input.Height+",\"frames\":"+bounds.Count+",\"sourceEdgeCells\":"+edgeCells+",\"transparentPixels\":"+transparent+",\"scale\":"+scale.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"clippedFrames\":"+clipped+"}";
    }
  }
}
'@ -ReferencedAssemblies @('System.Drawing','System.Drawing.Primitives','System.Drawing.Common','System.Private.Windows.GdiPlus','System.Private.Windows.Core','System.Linq','System.Collections','System.Console','System.Runtime.InteropServices')
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$jobs = Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json
$report = @()
foreach ($job in $jobs) {
  $sourceFolder = Join-Path $PSScriptRoot ('Sources\' + $job.id)
  New-Item -ItemType Directory -Path $sourceFolder -Force | Out-Null
  $sourceFile = Join-Path $sourceFolder ($job.form + '-source.png')
  if ([IO.Path]::GetFullPath($job.path) -ne [IO.Path]::GetFullPath($sourceFile)) { Copy-Item -LiteralPath $job.path -Destination $sourceFile -Force }
  $rows = if ($job.form -eq 'Human') { 4 } elseif ($job.form -eq 'Wolf') { 6 } else { 3 }
  $destination = Join-Path $projectRoot ('Assets\Characters\' + $job.id + '\' + $job.form + '\' + $job.id + '_' + $job.form + '_North.png')
  $measurement = [PackSheetLayout]::Normalize($sourceFile, $destination, $rows, ($job.form -eq 'VFX')) | ConvertFrom-Json
  $report += [pscustomobject]@{character=$job.id;form=$job.form;destination=$destination;measurement=$measurement}
  Write-Output ($job.id + ' ' + $job.form + ': ' + $measurement.frames + ' frames; alpha verified; source edge cells=' + $measurement.sourceEdgeCells)
}
$report | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'layout-audit.json') -Encoding utf8

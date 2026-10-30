$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
public static class PackCardArtExport {
  public static void Cell(string source,string destination,int columns,int rows,int index,int width,int height,bool trim) {
    using(var image=new Bitmap(source)) {
      int col=index%columns,row=index/columns;
      int x0=(int)Math.Round(col*image.Width/(double)columns),x1=(int)Math.Round((col+1)*image.Width/(double)columns);
      int y0=(int)Math.Round(row*image.Height/(double)rows),y1=(int)Math.Round((row+1)*image.Height/(double)rows);
      if(trim) {
        int left=x1,right=x0-1,top=y1,bottom=y0-1;
        for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)if(image.GetPixel(x,y).A>24){left=Math.Min(left,x);right=Math.Max(right,x);top=Math.Min(top,y);bottom=Math.Max(bottom,y);}
        if(right<left)throw new Exception("Empty cell: "+source+" "+index);
        x0=Math.Max(x0,left-2);y0=Math.Max(y0,top-2);x1=Math.Min(x1,right+3);y1=Math.Min(y1,bottom+3);
      }
      float padding=trim?Math.Min(width,height)*.025f:8;
      float scale=Math.Min((width-padding*2)/(x1-x0),(height-padding*2)/(y1-y0));
      using(var output=new Bitmap(width,height,PixelFormat.Format32bppArgb))using(var graphics=Graphics.FromImage(output))using(var attributes=new ImageAttributes()) {
        graphics.Clear(Color.Transparent);graphics.CompositingMode=CompositingMode.SourceCopy;graphics.InterpolationMode=InterpolationMode.HighQualityBicubic;graphics.PixelOffsetMode=PixelOffsetMode.HighQuality;attributes.SetWrapMode(WrapMode.TileFlipXY);
        float w=(x1-x0)*scale,h=(y1-y0)*scale;
        graphics.DrawImage(image,new RectangleF((width-w)*.5f,(height-h)*.5f,w,h),new RectangleF(x0,y0,x1-x0,y1-y0),GraphicsUnit.Pixel);
        Directory.CreateDirectory(Path.GetDirectoryName(destination));output.Save(destination,ImageFormat.Png);
      }
    }
  }
}
'@ -ReferencedAssemblies @('System.Drawing','System.Drawing.Primitives','System.Drawing.Common','System.Private.Windows.GdiPlus','System.Private.Windows.Core')
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$target = Join-Path $root 'Assets\UI\WorldOfThePack\Art'
$manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'generation-manifest.json') -Raw | ConvertFrom-Json
$paths = @{}
foreach ($source in $manifest.sources) {
  $local = Join-Path $PSScriptRoot ('Sources\' + $source.key + '.png')
  Copy-Item -LiteralPath $source.path -Destination $local -Force
  $paths[$source.key] = $local
}
[PackCardArtExport]::Cell($paths['CardFrame'],(Join-Path $target 'Frames\CardFrame.png'),1,1,0,768,1024,$true)
[PackCardArtExport]::Cell($paths['PortraitBackdrop'],(Join-Path $target 'Frames\PortraitBackdrop.png'),1,1,0,512,512,$false)
$controls = @('Nameplate','EnergyTrack','EnergyFill','DeployButton','ChargingButton','FormationTag','RepeatIcon','MoonshiftButton')
for ($index=0;$index -lt $controls.Count;$index++) {
  $size = if ($controls[$index] -eq 'RepeatIcon') { @(128,128) } else { @(512,160) }
  [PackCardArtExport]::Cell($paths['ControlAtlas'],(Join-Path $target ('Controls\' + $controls[$index] + '.png')),2,4,$index,$size[0],$size[1],$true)
}
foreach ($hero in $manifest.heroes) {
  [PackCardArtExport]::Cell($paths[$hero.id],(Join-Path $target ('Portraits\' + $hero.id + '_Human.png')),2,1,0,512,512,$true)
  [PackCardArtExport]::Cell($paths[$hero.id],(Join-Path $target ('Portraits\' + $hero.id + '_Wolf.png')),2,1,1,512,512,$true)
}
$effectNames=@('Charging','Deployed','Ready')
for($row=0;$row -lt 3;$row++){for($frame=0;$frame -lt 6;$frame++){
  [PackCardArtExport]::Cell($paths['UIVFX'],(Join-Path $target ('VFX\' + $effectNames[$row] + '_' + $frame.ToString('00') + '.png')),6,3,($row*6+$frame),128,128,$false)
}}
$audit = foreach($file in Get-ChildItem -LiteralPath $target -Recurse -Filter '*.png') {
  $bitmap=[System.Drawing.Bitmap]::new($file.FullName);$clear=0;$samples=0
  for($y=0;$y -lt $bitmap.Height;$y+=8){for($x=0;$x -lt $bitmap.Width;$x+=8){$samples++;if($bitmap.GetPixel($x,$y).A -eq 0){$clear++}}}
  [pscustomobject]@{file=$file.FullName.Substring($root.Length+1);width=$bitmap.Width;height=$bitmap.Height;transparentSamples=$clear;samples=$samples}
  $bitmap.Dispose()
}
$audit | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'QA\art-audit.json') -Encoding utf8
Write-Output ('Exported ' + $audit.Count + ' separate PNG assets, including 14 portraits and 18 UI VFX frames.')

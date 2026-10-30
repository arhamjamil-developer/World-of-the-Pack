$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
public static class PackCombatExport {
  public static void Frame(string source,string destination,int columns,int rows,int index,int[] edges) {
    using(var input=new Bitmap(source))using(var output=new Bitmap(256,256,PixelFormat.Format32bppArgb))using(var graphics=Graphics.FromImage(output)) {
      int col=index%columns,row=index/columns;
      int x0=(int)Math.Round(col*input.Width/(double)columns),x1=(int)Math.Round((col+1)*input.Width/(double)columns);
      int y0=(int)Math.Round(row*input.Height/(double)rows),y1=(int)Math.Round((row+1)*input.Height/(double)rows);
      if(edges!=null && edges.Length==rows+1){y0=edges[row];y1=edges[row+1];}
      graphics.Clear(Color.Transparent);graphics.CompositingMode=CompositingMode.SourceCopy;graphics.InterpolationMode=InterpolationMode.HighQualityBicubic;graphics.PixelOffsetMode=PixelOffsetMode.HighQuality;
      float scale=Math.Min(208f/(x1-x0),208f/(y1-y0));float w=(x1-x0)*scale,h=(y1-y0)*scale;
      using(var attributes=new ImageAttributes()){attributes.SetWrapMode(WrapMode.TileFlipXY);graphics.DrawImage(input,new RectangleF((256-w)*.5f,(256-h)*.5f,w,h),new RectangleF(x0,y0,x1-x0,y1-y0),GraphicsUnit.Pixel);}
      Directory.CreateDirectory(Path.GetDirectoryName(destination));output.Save(destination,ImageFormat.Png);
    }
  }
}
'@ -ReferencedAssemblies @('System.Drawing','System.Drawing.Primitives','System.Drawing.Common','System.Private.Windows.GdiPlus','System.Private.Windows.Core')
$root=(Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$manifest=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'generation-manifest-v02.json') -Raw | ConvertFrom-Json
$sequences=@();$frames=0
foreach($source in $manifest){
  $local=Join-Path $PSScriptRoot ('Sources\'+$source.id+'.png');Copy-Item -LiteralPath $source.path -Destination $local -Force
  for($row=0;$row -lt $source.rows.Count;$row++){
    $effectName=$source.rows[$row][0]
    $count=if($source.id -eq 'Telegraphs'){1}else{6}
    for($frame=0;$frame -lt $count;$frame++){
      $destination=Join-Path $root ('Assets\VFX\WorldOfThePack\Art\'+$source.id+'\'+$effectName+'_'+$frame.ToString('00')+'.png')
      if($source.id -eq 'Telegraphs'){[PackCombatExport]::Frame($local,$destination,2,3,$row,$null)}else{[PackCombatExport]::Frame($local,$destination,6,$source.rows.Count,($row*6+$frame),[int[]]$source.sourceRowEdges)}
      $frames++
    }
    $sorting=if($source.id -eq 'Telegraphs'){3}elseif($effectName -match 'Circle|Focus|Shockwave|Trap|Dust|Spawn|Rally|Barrier'){7}else{18}
    $sequences+=[pscustomobject]@{group=$source.id;name=$effectName;frames=$count;fps=if($source.fps){$source.fps}else{12};sortingOrder=$sorting;description=$source.rows[$row][1]}
  }
}
$sequences | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $root 'Assets\VFX\WorldOfThePack\Data\sequence-manifest.json') -Encoding utf8
$audit=foreach($file in Get-ChildItem -LiteralPath (Join-Path $root 'Assets\VFX\WorldOfThePack\Art') -Recurse -Filter '*.png'){
  $image=[System.Drawing.Bitmap]::new($file.FullName);$clear=0;$visible=0
  for($y=0;$y -lt 256;$y+=8){for($x=0;$x -lt 256;$x+=8){$a=$image.GetPixel($x,$y).A;if($a -eq 0){$clear++};if($a -gt 24){$visible++}}}
  if($clear -eq 0 -or $visible -eq 0){throw ('Invalid alpha or empty sprite: '+$file.FullName)}
  [pscustomobject]@{file=$file.FullName.Substring($root.Length+1);width=$image.Width;height=$image.Height;transparentSamples=$clear;visibleSamples=$visible}
  $image.Dispose()
}
$audit | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'QA\art-audit.json') -Encoding utf8
Write-Output ('Exported '+$frames+' transparent 256px sprites across '+$sequences.Count+' effect definitions.')

-- 5-4-3-2-1 Grounding. Estilo con contorno de color + brillos. Guarda como Grounding54321.aseprite

local function hex(h) return Color{ r=tonumber(h:sub(1,2),16), g=tonumber(h:sub(3,4),16), b=tonumber(h:sub(5,6),16), a=255 } end
local P = {
  bgTop=hex("46536A"), bgBot=hex("2B3545"), star=hex("FBE45C"), starH=hex("FFFFFF"),
  sk=hex("F0C8A0"), skM=hex("DBA074"), skS=hex("B97E54"), skD=hex("9A6240"), skH=hex("FBE3C4"), skO=hex("7A4A3A"), nail=hex("F6E0C6"),
  scl=hex("F6F0E4"), sclS=hex("D9D0BE"), irL=hex("8FC0E0"), ir=hex("4E86B8"), irD=hex("2E567E"), pup=hex("1A1E28"), lidO=hex("6E4634"),
  lipH=hex("EFA3B0"), lip=hex("C85E70"), lipD=hex("9E3F52"), mo=hex("5E2232"), teeth=hex("F6F0E4"), tg=hex("E27E92"), tgD=hex("C25F76"), moO=hex("5E2838"),
  wood=hex("7B5230"), woodL=hex("A06B3A"), woodO=hex("4B2E16"),
  cream=hex("FBEFD8"), parch=hex("F3E7CE"), parchO=hex("B79B6A")
}

local W,H = 256,176
local spr = Sprite(W,H, ColorMode.RGB)
spr.filename = "Grounding54321.aseprite"
local defaultLayer = spr.layers[1]

local function px(img,x,y,c) if x>=0 and y>=0 and x<img.width and y<img.height then img:drawPixel(x,y,c) end end
local function rf(img,x,y,w,h,c) for j=0,h-1 do for i=0,w-1 do px(img,x+i,y+j,c) end end end
local function disc(img,cx,cy,r,c) for j=-r,r do for i=-r,r do if i*i+j*j<=r*r then px(img,cx+i,cy+j,c) end end end end
local function ellip(img,cx,cy,rx,ry,c) for j=-ry,ry do for i=-rx,rx do if (i*i)*ry*ry+(j*j)*rx*rx<=rx*rx*ry*ry then px(img,cx+i,cy+j,c) end end end end
local function lerp(a,b,t) return Color{ r=math.floor(a.red+(b.red-a.red)*t+0.5), g=math.floor(a.green+(b.green-a.green)*t+0.5), b=math.floor(a.blue+(b.blue-a.blue)*t+0.5), a=255 } end
local function vgrad(img,x,y,w,h,ct,cb) for j=0,h-1 do local c=lerp(ct,cb,j/math.max(1,h-1)) for i=0,w-1 do px(img,x+i,y+j,c) end end end

local function outline(img,oc)
  local pts={}
  for y=0,H-1 do for x=0,W-1 do
    if app.pixelColor.rgbaA(img:getPixel(x,y))==0 then
      for _,d in ipairs({{1,0},{-1,0},{0,1},{0,-1}}) do local nx,ny=x+d[1],y+d[2]
        if nx>=0 and ny>=0 and nx<W and ny<H and app.pixelColor.rgbaA(img:getPixel(nx,ny))>0 then pts[#pts+1]={x,y} break end end
    end
  end end
  for _,p in ipairs(pts) do px(img,p[1],p[2],oc) end
end
local function layer(name, oc, fn) local lay=spr:newLayer() lay.name=name local img=Image(W,H) fn(img) if oc then outline(img,oc) end spr:newCel(lay,1,img,Point(0,0)) return lay end

layer("fondo_calmo", nil, function(img)
  vgrad(img,0,0,W,H,P.bgTop,P.bgBot)
  for _,s in ipairs({{206,30},{232,58},{190,86}}) do
    disc(img,s[1],s[2],2,P.star) px(img,s[1]-3,s[2],P.star) px(img,s[1]+3,s[2],P.star) px(img,s[1],s[2]-3,P.star) px(img,s[1],s[2]+3,P.star) px(img,s[1]-1,s[2]-1,P.starH)
  end
end)

layer("ojo", P.lidO, function(img)
  local cx,cy=32,34
  ellip(img,cx,cy,17,10,P.scl)
  ellip(img,cx,cy-3,17,6,P.sclS)
  disc(img,cx+1,cy,8,P.ir)
  for a=0,340,20 do px(img,cx+1+math.floor(7*math.cos(a*math.pi/180)+0.5),cy+math.floor(7*math.sin(a*math.pi/180)+0.5),P.irD) end
  disc(img,cx+1,cy,5,P.irL) disc(img,cx+1,cy,4,P.ir) disc(img,cx+1,cy,3,P.pup)
  disc(img,cx-1,cy-2,1,P.scl) px(img,cx+3,cy+2,P.irL)
  for k=-16,16 do px(img,cx+k,cy-10+math.floor(math.abs(k)*0.14),P.lidO) end
  for k=-14,14,3 do px(img,cx+k,cy-12,P.lidO) px(img,cx+k-1,cy-13,P.lidO) end
  for k=-15,15 do px(img,cx+k,cy+9-math.floor(math.abs(k)*0.12),P.skM) end
end)

layer("oido", P.skO, function(img)
  local cx,cy=80,34
  ellip(img,cx,cy,12,15,P.sk)
  for a=120,330,14 do px(img,cx+math.floor(10*math.cos(a*math.pi/180)+0.5),cy+math.floor(13*math.sin(a*math.pi/180)+0.5),P.skS) end
  ellip(img,cx+1,cy-1,7,10,P.skM)
  ellip(img,cx+2,cy,4,7,P.sk)
  for a=200,340,16 do px(img,cx+2+math.floor(5*math.cos(a*math.pi/180)+0.5),cy+math.floor(7*math.sin(a*math.pi/180)+0.5),P.skD) end
  ellip(img,cx-1,cy+10,4,4,P.sk)
  rf(img,cx-9,cy-11,3,12,P.skH)
end)

layer("nariz", P.skO, function(img)
  local cx,cy=128,36
  rf(img,cx-3,cy-13,6,15,P.sk)
  rf(img,cx-3,cy-13,2,15,P.skH) rf(img,cx+1,cy-13,2,15,P.skS)
  ellip(img,cx,cy+3,9,7,P.sk)
  ellip(img,cx-7,cy+4,4,5,P.skM) ellip(img,cx+7,cy+4,4,5,P.skM)
  disc(img,cx-5,cy+6,2,P.skD) disc(img,cx+5,cy+6,2,P.skD)
  disc(img,cx-1,cy,3,P.skH)
end)

layer("boca", P.moO, function(img)
  local cx,cy=176,36
  for k=-16,16 do px(img,cx+k,cy-5-math.floor((1-(k/16)^2)*3),P.lip) px(img,cx+k,cy-4-math.floor((1-(k/16)^2)*3),P.lipD) end
  ellip(img,cx,cy-1,15,5,P.mo)
  for k=-13,13 do px(img,cx+k,cy-5,P.teeth) px(img,cx+k,cy-4,P.teeth) end
  for k=-13,13,4 do px(img,cx+k,cy-5,P.sclS) end
  ellip(img,cx,cy+2,11,4,P.tg) px(img,cx,cy,P.tgD) px(img,cx,cy+1,P.tgD) px(img,cx,cy+2,P.tgD)
  rf(img,cx-7,cy+1,14,1,P.lipH)
  for k=-16,16 do px(img,cx+k,cy+4+math.floor((1-(k/16)^2)*4),P.lip) end
  rf(img,cx-11,cy+4,22,1,P.lipH)
end)

layer("tacto", P.skO, function(img)
  local cx,cy=216,36
  rf(img,cx-8,cy-4,16,18,P.sk) disc(img,cx,cy-4,8,P.sk)
  rf(img,cx-4,cy-9,8,5,P.nail)
  rf(img,cx-8,cy-4,3,18,P.skH) rf(img,cx+5,cy-4,3,18,P.skS)
  for r=2,8 do for a=200,520,16 do px(img,cx+math.floor(r*math.cos(a*math.pi/180)+0.5),cy+2+math.floor(r*0.85*math.sin(a*math.pi/180)+0.5),P.skD) end end
end)

layer("mano", P.skO, function(img)
  local cx,cy=120,108
  ellip(img,cx,cy+4,19,17,P.sk)
  rf(img,cx-17,cy-6,34,14,P.sk)
  local fx={cx-14,cx-5,cx+4,cx+14} local fh={26,30,29,24}
  for n=1,4 do local x=fx[n] local hh=fh[n]
    rf(img,x-3,cy-8-hh,7,hh,P.sk) disc(img,x,cy-8-hh,3,P.sk)
    rf(img,x-3,cy-8-hh,2,hh,P.skH) rf(img,x+2,cy-8-hh,2,hh,P.skS)
    rf(img,x-2,cy-9-hh,4,3,P.nail)
    px(img,x-4,cy-10,P.skD)
  end
  rf(img,cx-24,cy-4,11,8,P.sk) disc(img,cx-25,cy+1,4,P.sk) rf(img,cx-26,cy-3,3,7,P.skH) rf(img,cx-26,cy-2,3,3,P.nail)
  ellip(img,cx+5,cy+5,10,9,P.skM)
  for _,l in ipairs({{-11,2,9,0},{-9,9,11,2},{2,11,11,-2}}) do for k=0,l[3] do px(img,cx+l[1]+k,cy+l[2]+math.floor(k*l[4]/11),P.skD) end end
  rf(img,cx-16,cy-7,30,2,P.skH)
end)

layer("boton_volver", P.woodO, function(img) rf(img,8,152,70,20,P.cream) rf(img,8,152,70,2,hex("FFFFFF")) for k=0,4 do px(img,16+k,162-k,P.wood) px(img,16+k,162+k,P.wood) end rf(img,16,161,7,3,P.wood) end)
layer("boton_omitir", P.parchO, function(img) rf(img,90,152,70,20,P.parch) rf(img,90,152,70,2,P.cream) end)
layer("boton_siguiente", P.woodO, function(img) rf(img,172,152,76,20,P.wood) rf(img,172,152,76,2,P.woodL) end)

spr:deleteLayer(defaultLayer)
app.refresh()
app.alert("Grounding (estilo referencias) listo. File > Save As -> Grounding54321.aseprite")

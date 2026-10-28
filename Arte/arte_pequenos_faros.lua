-- Pequeños Faros  (arte verificado, cada elemento en su capa). Guarda como PequenosFaros.aseprite
local W,H=256,176
local spr=Sprite(W,H,ColorMode.RGB)
spr.filename='PequenosFaros.aseprite'
local defaultLayer=spr.layers[1]
local PAL={}
PAL['K']=Color{r=42,g=32,b=24,a=255}
PAL['k']=Color{r=74,g=58,b=44,a=255}
PAL['s']=Color{r=232,g=185,b=140,a=255}
PAL['l']=Color{r=246,g=215,b=176,a=255}
PAL['h']=Color{r=255,g=231,b=202,a=255}
PAL['d']=Color{r=201,g=152,b=107,a=255}
PAL['x']=Color{r=169,g=122,b=80,a=255}
PAL['n']=Color{r=243,g=220,b=192,a=255}
PAL['W']=Color{r=245,g=240,b=228,a=255}
PAL['w']=Color{r=216,g=207,b=187,a=255}
PAL['I']=Color{r=81,g=90,b=115,a=255}
PAL['i']=Color{r=123,g=134,b=166,a=255}
PAL['D']=Color{r=45,g=51,b=68,a=255}
PAL['P']=Color{r=20,g=18,b=23,a=255}
PAL['H']=Color{r=255,g=255,b=255,a=255}
PAL['p']=Color{r=227,g=154,b=176,a=255}
PAL['q']=Color{r=207,g=127,b=149,a=255}
PAL['U']=Color{r=184,g=80,b=104,a=255}
PAL['u']=Color{r=224,g=126,b=146,a=255}
PAL['m']=Color{r=242,g=168,b=182,a=255}
PAL['M']=Color{r=122,g=46,b=62,a=255}
PAL['T']=Color{r=229,g=143,b=160,a=255}
PAL['t']=Color{r=201,g=106,b=130,a=255}
PAL['E']=Color{r=243,g=237,b=224,a=255}
PAL['o']=Color{r=224,g=160,b=90,a=255}
PAL['O']=Color{r=184,g=120,b=56,a=255}
PAL['G']=Color{r=91,g=158,b=71,a=255}
PAL['g']=Color{r=140,g=200,b=120,a=255}
PAL['J']=Color{r=63,g=122,b=48,a=255}
PAL['f']=Color{r=198,g=231,b=186,a=255}
PAL['R']=Color{r=196,g=124,b=76,a=255}
PAL['r']=Color{r=154,g=94,b=53,a=255}
PAL['Y']=Color{r=218,g=154,b=107,a=255}
PAL['z']=Color{r=90,g=59,b=34,a=255}
PAL['Q']=Color{r=176,g=110,b=63,a=255}
PAL['A']=Color{r=134,g=174,b=220,a=255}
PAL['a']=Color{r=87,g=127,b=179,a=255}
PAL['b']=Color{r=180,g=208,b=240,a=255}
PAL['V']=Color{r=127,g=196,b=228,a=255}
PAL['v']=Color{r=205,g=238,b=246,a=255}
PAL['N']=Color{r=123,g=82,b=48,a=255}
PAL['L']=Color{r=160,g=107,b=58,a=255}
PAL['C']=Color{r=251,g=239,b=216,a=255}
PAL['c']=Color{r=243,g=231,b=206,a=255}
PAL['e']=Color{r=224,g=207,b=164,a=255}
PAL['B']=Color{r=187,g=209,b=238,a=255}
PAL['0']=Color{r=107,g=113,b=135,a=255}
PAL['F']=Color{r=246,g=203,b=212,a=255}
PAL['1']=Color{r=237,g=231,b=218,a=255}
PAL['2']=Color{r=201,g=194,b=176,a=255}
PAL['S']=Color{r=248,g=206,b=99,a=255}
PAL['Z']=Color{r=242,g=201,b=76,a=255}
PAL['@']=Color{r=94,g=58,b=30,a=255}
PAL['%']=Color{r=155,g=111,b=201,a=255}
PAL['$']=Color{r=229,g=132,b=168,a=255}
PAL['^']=Color{r=224,g=160,b=90,a=255}
PAL['3']=Color{r=216,g=90,b=48,a=255}
PAL['4']=Color{r=176,g=70,b=42,a=255}
PAL['5']=Color{r=74,g=85,b=112,a=255}
PAL['6']=Color{r=52,g=61,b=84,a=255}
PAL['7']=Color{r=176,g=70,b=42,a=255}
PAL['8']=Color{r=247,g=212,b=90,a=255}
PAL['9']=Color{r=251,g=228,b=140,a=255}
PAL['<']=Color{r=27,g=39,b=64,a=255}
PAL['>']=Color{r=44,g=60,b=92,a=255}
PAL['+']=Color{r=251,g=224,b=138,a=255}
local function lerp(a,b,t) return Color{r=math.floor(a.red+(b.red-a.red)*t+0.5),g=math.floor(a.green+(b.green-a.green)*t+0.5),b=math.floor(a.blue+(b.blue-a.blue)*t+0.5),a=255} end
local function hexC(h) return Color{r=tonumber(h:sub(1,2),16),g=tonumber(h:sub(3,4),16),b=tonumber(h:sub(5,6),16),a=255} end
local function bg(name, ct, cb, extra)
  local lay=spr:newLayer() lay.name=name
  local img=Image(W,H)
  for j=0,H-1 do local c=lerp(ct,cb,j/(H-1)) for i=0,W-1 do img:drawPixel(i,j,c) end end
  if extra then extra(img) end
  spr:newCel(lay,1,img,Point(0,0))
end
local function grid(name, ox, oy, rows)
  local lay=spr:newLayer() lay.name=name
  local img=Image(W,H)
  for yy=1,#rows do local row=rows[yy]
    for xx=1,#row do local ch=row:sub(xx,xx) local c=PAL[ch]
      if c then img:drawPixel(ox+xx-1, oy+yy-1, c) end
    end
  end
  spr:newCel(lay,1,img,Point(0,0))
end

bg("fondo", hexC("3A4A6E"), hexC("242E46"), function(img)
  for j=H-44,H-1 do for i=0,W-1 do img:drawPixel(i,j,hexC("1B2740")) end end
  for x=0,W-1 do img:drawPixel(x,H-44+math.floor(2*math.sin(x/10)),hexC("2C3C5C")) end
  local function st(x,y) img:drawPixel(x,y,hexC("FBEFD8")) end
  st(90,20) st(140,30) st(190,18) st(220,40) st(110,46)
end)
grid('luna',28,20,{
  "                      ",
  "                      ",
  "       KK             ",
  "      KCCK            ",
  "     KeCK             ",
  "    KeeCK             ",
  "   KeeeK              ",
  "  KeeeeK              ",
  "  KeeeeK              ",
  "  KeeeK               ",
  "  KeeeeK              ",
  " KeeeeeK              ",
  "  KeeeeK              ",
  "  KeeeeCK             ",
  "  KeeeeCK             ",
  "  KeeeeCCKK       K   ",
  "   KeeeCCCCKKK KKKCK  ",
  "    KeeCCCCCCCKCCCK   ",
  "     KeCCCCCCCCCCK    ",
  "      KCCCCCCCCCK     ",
  "       KKKKCKKKK      ",
  "           K          "
})

grid('haz_luz',150,60,{
  "                                                                      ",
  "                                                                      ",
  "                                                                      ",
  "                                                                      ",
  "                                                                      ",
  "                                                                      ",
  "                                                                      ",
  "                                                               KKK    ",
  "                                                            KKK999K   ",
  "                                                        KKKK999999K   ",
  "                                                     KKK9999999999K   ",
  "                                                  KKK9999999999999K   ",
  "                                              KKKK9999999999999999K   ",
  "                                           KKK99999999999999999999K   ",
  "                                        KKK99999999999999999999999K   ",
  "                                    KKKK99999999999999999999999999K   ",
  "                                 KKK999999999999999999999999999999K   ",
  "                              KKK999999999999999999999999999999999K   ",
  "                          KKKK999999999999999999999999999999999999K   ",
  "                       KKK9999999999999999999999999999999999999999K   ",
  "                    KKK9999999999999999999999999999999999999999999K   ",
  "                KKKK9999999999999999999999999999999999999999999999K   ",
  "             KKK99999999999999999999999999999999999999999999999999K   ",
  "          KKK99999999999999999999999999999999999999999999999999999K   ",
  "      KKKK99999999999999999999999999999999999999999999999999999999K   ",
  "     K999999999999999999999999999999999999999999999999999999999999K   ",
  "      KKKK99999999999999999999999999999999999999999999999999999999K   ",
  "          KKK99999999999999999999999999999999999999999999999999999K   ",
  "             KKK99999999999999999999999999999999999999999999999999K   ",
  "                KKKK9999999999999999999999999999999999999999999999K   ",
  "                    KKK9999999999999999999999999999999999999999999K   ",
  "                       KKK9999999999999999999999999999999999999999K   ",
  "                          KKKK999999999999999999999999999999999999K   ",
  "                              KKK999999999999999999999999999999999K   ",
  "                                 KKK999999999999999999999999999999K   ",
  "                                    KKKK99999999999999999999999999K   ",
  "                                        KKK99999999999999999999999K   ",
  "                                           KKK99999999999999999999K   ",
  "                                              KKKK9999999999999999K   ",
  "                                                  KKK9999999999999K   ",
  "                                                     KKK9999999999K   ",
  "                                                        KKKK999999K   ",
  "                                                            KKK999K   ",
  "                                                               KKK    ",
  "                                                                      ",
  "                                                                      ",
  "                                                                      ",
  "                                                                      ",
  "                                                                      ",
  "                                                                      "
})

grid('faro_apagado',44,60,{
  "                      K3K                     ",
  "                     K333K                    ",
  "                    K33333K                   ",
  "             KKKKKKKKK333KKKKKKKK             ",
  "            K77777777773777777777K            ",
  "             KK7777777777777777KK             ",
  "               K77777777777777K               ",
  "                K777777777777K                ",
  "                 K7777777777K                 ",
  "                  K77777777K                  ",
  "                   K777777K                   ",
  "              KKKKKKK7777KKKKKKK              ",
  "             K555555557755555555K             ",
  "             K555555555555555555K             ",
  "             K555555555555555555K             ",
  "             K555555555555555555K             ",
  "             K555555555555555555K             ",
  "              KK00000000000000KK              ",
  "               K00000000000000K               ",
  "            KKKK00000000000000KKKK            ",
  "           K1111000000000000001111K           ",
  "            K11100000000000000111K            ",
  "             K110000000000000011K             ",
  "              K1000000000000001K              ",
  "               K00000000000000K               ",
  "               K00000000000000K               ",
  "               K00000000000000K               ",
  "               K00000000000000K               ",
  "               K00000000000000K               ",
  "               K00000000000000K               ",
  "               K00000000000000K               ",
  "            KKKK00000000000000KKKK            ",
  "           K5555555555555555555555K           ",
  "           K5555555555555555555555K           ",
  "           K5555555555555555555555K           ",
  "           K5555555555555555555555K           ",
  "            KKK1111111111112222KKK            ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              KCCCCCCCCCCCCCCCCK              ",
  "              KCCCCCCCCCCCCCCCCK              ",
  "              K3333333333333333K              ",
  "              K3333333333333333K              ",
  "              K3333333333333333K              ",
  "              K3333333333333333K              ",
  "              K4444444444444444K              ",
  "              K4444444444444444K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              KCCCCCCCCCCCCCCCCK              ",
  "              KCCCCCCCCCCCCCCCCK              ",
  "              K3333333333333333K              ",
  "              K3333333333333333K              ",
  "              K3333333333333333K              ",
  "              K3333333333333333K              ",
  "              K4444444444444444K              ",
  "              K4444444444444444K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              KCCCCCCCCCCCCCCCCK              ",
  "              KCCCCCCCCCCCCCCCCK              ",
  "              K3333333333333333K              ",
  "              K3333333333333333K              ",
  "              K3333333333333333K              ",
  "              K3333333333333333K              ",
  "              K4444444444444444K              ",
  "              K4444444444444444K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "               KKKKKKKKKKKKKKKK               ",
  "                                              ",
  "                                              ",
  "                                              ",
  "                                              ",
  "                                              "
})

grid('faro_encendido',150,60,{
  "                      K3K                     ",
  "                     K333K                    ",
  "                    K33333K                   ",
  "             KKKKKKKKK333KKKKKKKK             ",
  "            K77777777773777777777K            ",
  "             KK7777777777777777KK             ",
  "               K77777777777777K               ",
  "                K777777777777K                ",
  "                 K7777777777K                 ",
  "                  K77777777K                  ",
  "                   K777777K                   ",
  "              KKKKKKK7777KKKKKKK              ",
  "             K555555557755555555K             ",
  "             K555555555555555555K             ",
  "             K555555555555555555K             ",
  "             K555555555555555555K             ",
  "             K555555555555555555K             ",
  "              KK88888888888888KK              ",
  "               K88888888888888K               ",
  "            KKKK88888888888888KKKK            ",
  "           K1111888888898888881111K           ",
  "            K11188888999998888111K            ",
  "             K118888999999988811K             ",
  "              K1888899999998881K              ",
  "               K88899999999988K               ",
  "               K88889999999888K               ",
  "               K88889999999888K               ",
  "               K88888999998888K               ",
  "               K88888889888888K               ",
  "               K88888888888888K               ",
  "               K88888888888888K               ",
  "            KKKK88888888888888KKKK            ",
  "           K5555555555555555555555K           ",
  "           K5555555555555555555555K           ",
  "           K5555555555555555555555K           ",
  "           K5555555555555555555555K           ",
  "            KKK1111111111112222KKK            ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              KCCCCCCCCCCCCCCCCK              ",
  "              KCCCCCCCCCCCCCCCCK              ",
  "              K3333333333333333K              ",
  "              K3333333333333333K              ",
  "              K3333333333333333K              ",
  "              K3333333333333333K              ",
  "              K4444444444444444K              ",
  "              K4444444444444444K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              KCCCCCCCCCCCCCCCCK              ",
  "              KCCCCCCCCCCCCCCCCK              ",
  "              K3333333333333333K              ",
  "              K3333333333333333K              ",
  "              K3333333333333333K              ",
  "              K3333333333333333K              ",
  "              K4444444444444444K              ",
  "              K4444444444444444K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "              KCCCCCCCCCCCCCCCCK              ",
  "              KCCCCCCCCCCCCCCCCK              ",
  "              K3333333333333333K              ",
  "              K3333333333333333K              ",
  "              K3333333333333333K              ",
  "              K3333333333333333K              ",
  "              K4444444444444444K              ",
  "              K4444444444444444K              ",
  "              K1111111111112222K              ",
  "              K1111111111112222K              ",
  "               KKKKKKKKKKKKKKKK               ",
  "                                              ",
  "                                              ",
  "                                              ",
  "                                              ",
  "                                              "
})

grid('boton_volver',10,10,{
  "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC",
  "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC",
  "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "ccccccccccccccccccccNccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "cccccccccccccccccccNcccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "ccccccccccccccccccNccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "ccccccccccccccccNNNNNNNNcccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "ccccccccccccccccNNNNNNNNcccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "ccccccccccccccccNNNNNNNNcccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "ccccccccccccccccccNccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "cccccccccccccccccccNcccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "ccccccccccccccccccccNccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
  "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc"
})

spr:deleteLayer(defaultLayer)
app.refresh()
app.alert('Pequeños Faros listo. File > Save As -> PequenosFaros.aseprite')
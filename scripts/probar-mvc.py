from pathlib import Path
from http.cookiejar import CookieJar
from urllib.request import Request,build_opener,HTTPCookieProcessor
from urllib.error import HTTPError
from urllib.parse import urlencode
from lxml import html
import random,json,html as html_std
ROOT=Path(__file__).resolve().parents[1]
lines=[]
def check(ok,label):
 if not ok:raise AssertionError(label)
 lines.append('OK '+label);print(lines[-1],flush=True)
def api(path,source):
 req=Request('http://localhost:5080/api/'+path,headers={'X-Fuente-Datos':source})
 try:r=build_opener().open(req,timeout=30)
 except HTTPError as e:r=e
 data=r.read();return r.status,json.loads(data) if data else None
snap={s:{t:api(t,s)[1] for t in ['automoviles','parqueos','ingresos']} for s in ['json','postgres']}
for port,maintenance,query in [(5090,'json','postgres'),(5091,'postgres','json')]:
 label=f'MVC {port} ({maintenance}/{query})';base=f'http://localhost:{port}'
 client=build_opener(HTTPCookieProcessor(CookieJar()))
 def call(path,data=None):
  req=Request(base+path,data=urlencode(data).encode() if data is not None else None)
  try:r=client.open(req,timeout=40)
  except HTTPError as e:r=e
  raw=r.read().decode('utf-8');return r.status,raw,r.url
 def token(path='/Automoviles/Crear'):
  code,raw,_=call(path);assert code==200
  return html.fromstring(raw).xpath('//input[@name="__RequestVerificationToken"]/@value')[0]
 def post(path,data):return call(path,{**data,'__RequestVerificationToken':token()})
 key=random.randrange(200000000,290000000)
 check(api(f'automoviles/{key}',maintenance)[0]==404,label+' clave temporal libre')
 created=False
 try:
  code,raw,_=call('/');check(code==200 and len(html.fromstring(raw).xpath('//tr[@data-vehicle-id]'))==5,label+' lista inicial')
  check(('Archivo JSON' if maintenance=='json' else 'PostgreSQL') in raw,label+' fuente de mantenimiento visible')
  code,raw,_=call('/?Desde=2020&Hasta=2022');check(len(html.fromstring(raw).xpath('//tr[@data-vehicle-id]'))==2,label+' intervalo inclusivo')
  code,raw,_=call('/?Desde=2020&Hasta=2020');check(len(html.fromstring(raw).xpath('//tr[@data-vehicle-id]'))==1,label+' límite exacto')
  code,raw,_=call('/?Desde=2025&Hasta=2025');check('No hay vehículos' in html_std.unescape(raw),label+' consulta vacía')
  code,raw,_=call('/?Desde=2024&Hasta=2020');check('inicial no puede ser mayor' in html.fromstring(raw).text_content(),label+' rango invertido')
  code,raw,_=call('/?Desde=no-numero&Hasta=2020');check('validation-summary-errors' in raw,label+' parámetro no numérico')
  code,raw,_=call('/?Desde=&Hasta=2020');check('Ingrese los dos' in raw,label+' límite vacío')
  a={'Id':key,'Color':'Verde','Anio':2023,'Fabricante':'Prueba TC04 <seguro>','Tipo':'moto'}
  check(call('/Automoviles/Crear',a)[0]==400,label+' POST sin antifalsificación rechazado')
  code,raw,_=post('/Automoviles/Crear',a);created=api(f'automoviles/{key}',maintenance)[0]==200
  check(code==200 and created and 'creado correctamente' in raw,label+' crear a través de API')
  check(api(f'automoviles/{key}','postgres' if maintenance=='json' else 'json')[0]==404,label+' escritura solo en fuente configurada')
  code,raw,_=call(f'/Automoviles/Detalles/{key}');check(code==200 and '&lt;seguro&gt;' in raw and '<seguro>' not in raw,label+' detalles y escape HTML')
  code,raw,_=post('/Automoviles/Crear',{**a,'Id':1});check('validation-summary-errors' in raw,label+' no duplicar clave')
  code,raw,_=post(f'/Automoviles/Editar/{key}',{**a,'Anio':1800});check('validation-summary-errors' in raw and api(f'automoviles/{key}',maintenance)[1]['anio']==2023,label+' validación servidor sin persistir')
  code,raw,_=post(f'/Automoviles/Editar/{key}',{**a,'Id':key+1});check('no coincide' in raw,label+' clave URL/formulario consistente')
  code,raw,_=post(f'/Automoviles/Editar/{key}',{**a,'Color':'Gris'});check(code==200 and api(f'automoviles/{key}',maintenance)[1]['color']=='Gris',label+' editar a través de API')
  code,raw,_=call(f'/Automoviles/Eliminar/{key}');check('Confirmar eliminación' in html_std.unescape(raw) and api(f'automoviles/{key}',maintenance)[0]==200,label+' confirmar antes de eliminar')
  code,raw,_=call('/Automoviles/Eliminar/1');check('No se puede eliminar' in raw and 'Confirmar eliminación' not in html_std.unescape(raw),label+' vehículo relacionado protegido en pantalla')
  code,raw,_=post('/Automoviles/Eliminar/1',{});check('validation-summary-errors' in raw and api('automoviles/1',maintenance)[0]==200,label+' protección al forzar borrado relacionado')
  code,raw,_=post(f'/Automoviles/Eliminar/{key}',{});check(code==200 and api(f'automoviles/{key}',maintenance)[0]==404,label+' eliminar a través de API');created=False
  for action in ['Detalles','Editar','Eliminar']:check(call(f'/Automoviles/{action}/{key}')[0]==404,label+' '+action+' inexistente')
  for tipo,n in [('sedán',2),('4x4',2),('moto',1)]:
   code,raw,_=call('/Consultas?'+urlencode({'tipo':tipo}));check(code==200 and len(html.fromstring(raw).xpath('//tr[@data-master-id]'))==n,label+' maestro '+tipo)
  check(('PostgreSQL' if query=='postgres' else 'Archivo JSON') in raw,label+' fuente de consulta visible')
  code,raw,_=call('/Consultas?tipo=camion');check('Seleccione un tipo' in raw,label+' combo no admite tipos inválidos')
  code,raw,_=call('/Consultas/Ingresos/4');d=json.loads(raw)
  check(code==200 and d['fuente']==query and len(d['ingresos'])==3,label+' detalle desde API filtrada')
  closed=d['ingresos'][0];opened=d['ingresos'][-1]
  check(closed['duracionMinutos']==180 and closed['duracionHoras']==3 and closed['montoTotal']==3600,label+' minutos horas y monto')
  check(all(opened[k] is None for k in ['salida','duracionMinutos','duracionHoras','montoTotal']),label+' ingreso abierto con nulos')
  check(call('/Consultas/Ingresos/2147483647')[0]==404,label+' maestro inexistente')
  check(call('/Consultas/Ingresos/0')[0]==400,label+' maestro con clave inválida')
 finally:
  if created:
   status,_,_=post(f'/Automoviles/Eliminar/{key}',{});assert api(f'automoviles/{key}',maintenance)[0]==404,('limpieza',status)
for source in snap:
 for entity,data in snap[source].items():check(api(entity,source)[1]==data,f'{source} {entity}: datos originales intactos')
report=f'{len(lines)} comprobaciones MVC por HTTP correctas.\n'+'\n'.join(lines)+'\n'
(ROOT/'docs/trabajo-04/verificacion-mvc.txt').write_text(report,encoding='utf-8')
print(report.splitlines()[0])

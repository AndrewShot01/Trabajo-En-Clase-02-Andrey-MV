from pathlib import Path
import json,urllib.request,urllib.error,random,sys
ROOT=Path(__file__).resolve().parents[1]
BASE='http://localhost:5080'
get_cases=[
 ('Automóviles todos','automoviles',5),('Automóvil por clave y navegación','automoviles/1',None),
 ('Automóviles por color','automoviles/por-color?texto=roj',2),('Automóviles por años','automoviles/por-anios?desde=2020&hasta=2022',2),
 ('Automóviles por fabricante','automoviles/por-fabricante?texto=yot',2),('Automóviles por tipo','automoviles/por-tipo?texto=4x',2),
 ('Parqueos todos','parqueos',2),('Parqueo por clave y navegación','parqueos/1',None),
 ('Parqueos por provincia','parqueos/por-provincia?texto=Jos%C3%A9',1),('Parqueos por nombre','parqueos/por-nombre?texto=flor',1),
 ('Parqueos por precio','parqueos/por-precios?desde=900&hasta=1200',2),
 ('Ingresos todos','ingresos',15),('Ingreso cerrado y relaciones','ingresos/1',None),('Ingreso abierto y cálculos nulos','ingresos/14',None),
 ('Ingresos por tipo','ingresos/por-tipo?texto=sed&desde=2026-10-01T00:00:00&hasta=2026-10-16T00:00:00',6),
 ('Ingresos por provincia','ingresos/por-provincia?texto=here&desde=2026-10-01T00:00:00&hasta=2026-10-16T00:00:00',8),
 ('Precio por hora','ingresos/precio-por-hora/1',None)]

def cases(key):
 a={'id':key,'color':'Verde','anio':2023,'fabricante':'Prueba API TC3','tipo':'moto'}
 p={'id':key,'provincia':'Cartago','nombre':'Prueba API TC3','precioPorHora':600}
 i={'consecutivo':key,'idParqueo':key,'idAutomovil':key,'entrada':'2026-10-20T08:00:00','salida':None}
 rows=[(name,'GET',path,200,None,n) for name,path,n in get_cases]
 rows += [(f'Precondición clave libre {entity}','GET',f'{entity}/{key}',404,None,None) for entity in ['automoviles','parqueos','ingresos']]
 rows += [
 ('Crear automóvil','POST','automoviles',201,a,None),('Clave duplicada','POST','automoviles',409,a,None),
 ('Actualizar automóvil','PUT',f'automoviles/{key}',204,{**a,'color':'Gris'},None),('Ver automóvil actualizado','GET',f'automoviles/{key}',200,None,None),
 ('Crear parqueo','POST','parqueos',201,p,None),('Actualizar parqueo','PUT',f'parqueos/{key}',204,{**p,'precioPorHora':900},None),('Ver parqueo actualizado','GET',f'parqueos/{key}',200,None,None),
 ('Crear ingreso abierto','POST','ingresos',201,i,None),('Ver ingreso nuevo','GET',f'ingresos/{key}',200,None,None),
 ('Actualizar salida del ingreso','PUT',f'ingresos/{key}',204,{**i,'salida':'2026-10-20T10:00:00'},None),('Ver monto del ingreso actualizado','GET',f'ingresos/{key}',200,None,None),
 ('No borrar automóvil relacionado','DELETE',f'automoviles/{key}',409,None,None),('No borrar parqueo relacionado','DELETE',f'parqueos/{key}',409,None,None),
 ('No aceptar salida anterior','PUT',f'ingresos/{key}',400,{**i,'salida':'2026-10-20T07:00:00'},None),
 ('No aceptar relación inexistente','POST','ingresos',400,{**i,'consecutivo':2147483646,'idAutomovil':2147483647},None),
 ('Borrar ingreso temporal','DELETE',f'ingresos/{key}',204,None,None),('Borrar automóvil temporal','DELETE',f'automoviles/{key}',204,None,None),('Borrar parqueo temporal','DELETE',f'parqueos/{key}',204,None,None),
 ('Comprobar ingreso borrado','GET',f'ingresos/{key}',404,None,None),('Comprobar automóvil borrado','GET',f'automoviles/{key}',404,None,None),('Comprobar parqueo borrado','GET',f'parqueos/{key}',404,None,None),
 ('Rango invertido','GET','automoviles/por-anios?desde=2024&hasta=2020',400,None,None),
 ('Dato inválido de automóvil','POST','automoviles',400,{**a,'anio':0},None),
 ('URL y cuerpo con claves diferentes','PUT','automoviles/1',400,a,None),
 ('Actualizar registro inexistente','PUT',f'automoviles/{key}',404,a,None),('Borrar registro inexistente','DELETE',f'parqueos/{key}',404,None,None),
 ('Parámetro obligatorio ausente','GET','automoviles/por-color',400,None,None)]
 return rows

def assertions(name):
 checks={
 'Automóvil por clave y navegación':'pm.expect(d.ingresos.length).to.eql(3);',
 'Parqueo por clave y navegación':'pm.expect(d.ingresos.length).to.eql(7);',
 'Ingreso cerrado y relaciones':'pm.expect(d.automovil.id).to.eql(d.idAutomovil); pm.expect(d.parqueo.id).to.eql(d.idParqueo); pm.expect(d.duracionMinutos).to.eql(45); pm.expect(d.duracionHoras).to.eql(0.75); pm.expect(d.montoTotal).to.eql(675);',
 'Ingreso abierto y cálculos nulos':'pm.expect(d.salida).to.eql(null); pm.expect(d.duracionMinutos).to.eql(null); pm.expect(d.duracionHoras).to.eql(null); pm.expect(d.montoTotal).to.eql(null);',
 'Precio por hora':'pm.expect(d.precioPorHora).to.eql(1200);',
 'Ver automóvil actualizado':'pm.expect(d.color).to.eql("Gris");',
 'Ver parqueo actualizado':'pm.expect(d.precioPorHora).to.eql(900);',
 'Ver monto del ingreso actualizado':'pm.expect(d.montoTotal).to.eql(1800);',
 'Ver ingreso nuevo':'pm.expect(d.montoTotal).to.eql(null); pm.expect(d.automovil.id).to.eql(d.idAutomovil);',
 }
 return checks.get(name)

collection={'info':{'name':'Trabajo en clase 3 — API Parqueo','schema':'https://schema.getpostman.com/json/collection/v2.1.0/collection.json','description':'API local. Ejecutar las carpetas completas en orden. Cada fuente usa una clave temporal y elimina sus registros al terminar. No contiene credenciales.'},
 'variable':[{'key':'baseUrl','value':BASE},{'key':'clavePrueba','value':'190000003'}],'item':[]}
for source in ['json','postgres']:
 folder={'name':source.upper(),'item':[]}
 for idx,(name,method,path,status,body,n) in enumerate(cases('{{clavePrueba}}')):
  raw=json.dumps(body,ensure_ascii=False).replace('"{{clavePrueba}}"','{{clavePrueba}}') if body else None
  tests=[f'pm.test("HTTP {status}", () => pm.response.to.have.status({status}));',f'pm.test("Fuente {source}", () => pm.expect(pm.response.headers.get("X-Fuente-Datos")).to.eql("{source}"));']
  if n is not None:tests.append(f'pm.test("Cantidad {n}", () => pm.expect(pm.response.json().length).to.eql({n}));')
  if assertions(name):tests.append('pm.test("Contenido y navegación", () => { const d=pm.response.json(); '+assertions(name)+' });')
  if name.startswith('Precondición'):tests.append('if(pm.response.code!==404) { pm.execution.setNextRequest(null); throw new Error("Clave ocupada: detener para no alterar registros existentes."); }')
  item={'name':name,'request':{'method':method,'url':'{{baseUrl}}/api/'+path,'header':[{'key':'X-Fuente-Datos','value':source}]},'event':[{'listen':'test','script':{'type':'text/javascript','exec':tests}}]}
  if idx==0:item['event'].append({'listen':'prerequest','script':{'type':'text/javascript','exec':['pm.collectionVariables.set("clavePrueba", String(100000000+Math.floor(Math.random()*90000000)));']}})
  if raw:item['request'].update({'header':item['request']['header']+[{'key':'Content-Type','value':'application/json'}],'body':{'mode':'raw','raw':raw,'options':{'raw':{'language':'json'}}}})
  folder['item'].append(item)
 collection['item'].append(folder)
(ROOT/'postman/Trabajo-03-Parqueo.postman_collection.json').write_text(json.dumps(collection,ensure_ascii=False,indent=2),encoding='utf-8')

if '--solo-coleccion' in sys.argv:sys.exit()
results=[]
for source in ['json','postgres']:
 key=random.randrange(100000000,190000000)
 before={}
 def call(method,path,body=None):
  req=urllib.request.Request(BASE+'/api/'+path,data=json.dumps(body,ensure_ascii=False).encode() if body else None,method=method,headers={'X-Fuente-Datos':source,'Content-Type':'application/json'})
  try:r=urllib.request.urlopen(req,timeout=30)
  except urllib.error.HTTPError as e:r=e
  data=r.read();return r.status,(json.loads(data) if 'json' in r.headers.get('Content-Type','') else data.decode()) if data else None,r.headers
 for entity in ['automoviles','parqueos','ingresos']:before[entity]=call('GET',entity)[1]
 created=[]
 try:
  for name,method,path,status,body,n in cases(key):
   actual,data,headers=call(method,path,body)
   if actual!=status:raise AssertionError(f'{source} {name}: esperado {status}, recibido {actual}: {data}')
   if headers.get('X-Fuente-Datos')!=source:raise AssertionError('Fuente incorrecta')
   if n is not None:assert len(data)==n,(name,len(data))
   if name=='Automóvil por clave y navegación':assert len(data['ingresos'])==3
   if name=='Parqueo por clave y navegación':assert len(data['ingresos'])==7
   if name=='Ingreso cerrado y relaciones':assert data['automovil']['id']==data['idAutomovil'] and data['parqueo']['id']==data['idParqueo'] and data['montoTotal']==675 and data['duracionMinutos']==45
   if name=='Ingreso abierto y cálculos nulos':assert all(data[k] is None for k in ['salida','duracionMinutos','duracionHoras','montoTotal'])
   if name=='Ver monto del ingreso actualizado':assert data['montoTotal']==1800
   if name=='Ver automóvil actualizado':assert data['color']=='Gris'
   if name=='Ver parqueo actualizado':assert data['precioPorHora']==900
   if method=='POST' and actual==201:created.append(path)
   results.append(f'OK {source.upper()} {method} {path} -> {actual} | {name}')
   print(results[-1],flush=True)
 finally:
  for entity in ['ingresos','automoviles','parqueos']:
   if entity in created:
    status,_,_=call('DELETE',f'{entity}/{key}')
    assert status in (204,404),('limpieza',entity,status)
 for entity in before:assert before[entity]==call('GET',entity)[1],f'Datos originales alterados {source} {entity}'
 results.append(f'OK {source.upper()}: datos originales conservados tras las pruebas HTTP.');print(results[-1],flush=True)
report=f'{len(results)-2} solicitudes HTTP verificadas; JSON y PostgreSQL. Datos originales conservados.\n'+'\n'.join(results)+'\n'
(ROOT/'docs/trabajo-03/verificacion-http.txt').write_text(report,encoding='utf-8')
print(report.splitlines()[0])

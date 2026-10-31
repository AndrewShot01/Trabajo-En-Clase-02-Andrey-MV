(() => {
 'use strict';
 const panel = document.getElementById('panel-estadias');
 const status = document.getElementById('estado-estadias');
 const title = document.getElementById('titulo-estadias');
 const body = document.getElementById('filas-estadias');
 const table = document.getElementById('tabla-estadias');
 const count = document.getElementById('contador-estadias');
 const buttons = [...document.querySelectorAll('.select-vehicle')];
 const numbers = new Intl.NumberFormat('es-CR', {maximumFractionDigits: 2});
 const money = new Intl.NumberFormat('es-CR', {style: 'currency', currency: 'CRC'});
 let activeRequest;
 function localDate(value) {
  if (value === null) return 'En el parqueo';
  const [date, time] = value.split('T');
  const [year, month, day] = date.split('-');
  return `${day}/${month}/${year} ${time.slice(0, 5)}`;
 }
 function cell(row, text, right = false) {
  const td = document.createElement('td'); td.textContent = text;
  if (right) td.className = 'right'; row.append(td);
 }
 buttons.forEach(button => button.addEventListener('click', async () => {
  activeRequest?.abort(); const current = new AbortController(); activeRequest = current;
  buttons.forEach(b => { b.setAttribute('aria-expanded', String(b === button)); b.closest('tr').classList.toggle('selected', b === button); });
  title.textContent = `${button.dataset.name} · #${button.dataset.id}`;
  status.textContent = 'Consultando las estadías…'; status.classList.remove('error-text');
  panel.setAttribute('aria-busy', 'true'); table.hidden = true; count.hidden = true; body.replaceChildren();
  try {
   const response = await fetch(button.dataset.url, {signal: current.signal, headers: {'Accept': 'application/json'}});
   const data = await response.json();
   if (!response.ok) throw new Error(data.mensaje || 'No se pudo consultar el historial. Intente de nuevo.');
   if (activeRequest !== current) return;
   count.textContent = `${data.ingresos.length} ingreso(s)`; count.hidden = false;
   if (!data.ingresos.length) { status.textContent = 'Este automóvil todavía no registra ingresos.'; return; }
   data.ingresos.forEach(stay => {
    const row = document.createElement('tr');
    cell(row, `#${stay.idParqueo} · ${stay.parqueo.nombre}`);
    cell(row, localDate(stay.entrada)); cell(row, localDate(stay.salida));
    cell(row, stay.duracionMinutos === null ? '—' : numbers.format(stay.duracionMinutos), true);
    cell(row, stay.duracionHoras === null ? '—' : numbers.format(stay.duracionHoras), true);
    cell(row, stay.montoTotal === null ? 'Pendiente de salida' : money.format(stay.montoTotal), true); body.append(row);
   });
   status.textContent = 'Los ingresos sin salida no tienen todavía duración ni monto calculados.'; table.hidden = false;
  } catch (error) {
   if (error.name === 'AbortError' || activeRequest !== current) return;
   status.textContent = error instanceof TypeError ? 'No se pudo conectar. Verifica que la web y la API estén iniciadas.' : error.message;
   status.classList.add('error-text');
  } finally { if (activeRequest === current) panel.setAttribute('aria-busy', 'false'); }
 }));
})();

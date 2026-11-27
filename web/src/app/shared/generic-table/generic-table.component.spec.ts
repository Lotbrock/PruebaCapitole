import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { GenericTableComponent } from './generic-table.component';

interface Person {
  name: string;
}

@Component({
  imports: [GenericTableComponent],
  template: `
    <app-generic-table
      [items]="items()"
      [emptyText]="emptyText()"
      [loading]="loading()"
      (rowClick)="clicked.set($event)"
    >
      <ng-template #header><tr><th>Nombre</th></tr></ng-template>
      <ng-template #row let-person><td>{{ person.name }}</td></ng-template>
    </app-generic-table>
  `,
})
class HostComponent {
  readonly items = signal<Person[]>([{ name: 'Ana' }, { name: 'Luis' }]);
  readonly emptyText = signal('Sin datos');
  readonly loading = signal(false);
  readonly clicked = signal<Person | null>(null);
}

describe('GenericTableComponent', () => {
  let fixture: ComponentFixture<HostComponent>;
  let host: HostComponent;

  const element = () => fixture.nativeElement as HTMLElement;
  const rows = () => Array.from(element().querySelectorAll<HTMLTableRowElement>('tbody tr'));

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [HostComponent] }).compileComponents();
    fixture = TestBed.createComponent(HostComponent);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('renders the header and one row per item', () => {
    expect(element().querySelector('thead th')?.textContent).toContain('Nombre');
    expect(rows().map((row) => row.textContent?.trim())).toEqual(['Ana', 'Luis']);
  });

  it('shows the empty text when there are no items', () => {
    host.items.set([]);
    fixture.detectChanges();

    expect(rows().length).toBe(1);
    expect(rows()[0].textContent).toContain('Sin datos');
  });

  it('shows a custom empty text', () => {
    host.items.set([]);
    host.emptyText.set('No hay reservas');
    fixture.detectChanges();

    expect(rows()[0].textContent).toContain('No hay reservas');
  });

  it('shows a loading message instead of the rows while loading', () => {
    host.loading.set(true);
    fixture.detectChanges();

    expect(element().textContent).toContain('Cargando');
    expect(element().textContent).not.toContain('Ana');
  });

  it('emits the item when a row is clicked', () => {
    rows()[1].click();

    expect(host.clicked()).toEqual({ name: 'Luis' });
  });

  it('emits the item when Enter is pressed on a row', () => {
    rows()[0].dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', bubbles: true }));

    expect(host.clicked()).toEqual({ name: 'Ana' });
  });
});

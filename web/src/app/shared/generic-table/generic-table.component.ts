import { NgTemplateOutlet } from '@angular/common';
import { Component, TemplateRef, contentChild, input, output } from '@angular/core';

/**
 * Generic table. It knows nothing about bookings: the caller provides the header and the row
 * through two templates.
 *
 *   <app-generic-table [items]="items" (rowClick)="open($event)">
 *     <ng-template #header><tr><th>Name</th></tr></ng-template>
 *     <ng-template #row let-item><td>{{ item.name }}</td></ng-template>
 *   </app-generic-table>
 */
@Component({
  selector: 'app-generic-table',
  imports: [NgTemplateOutlet],
  templateUrl: './generic-table.component.html',
  styleUrl: './generic-table.component.css',
})
export class GenericTableComponent<T> {
  readonly items = input.required<T[]>();
  readonly emptyText = input('Sin datos');
  readonly loading = input(false);
  readonly rowClick = output<T>();

  protected readonly headerTemplate = contentChild<TemplateRef<unknown>>('header');
  protected readonly rowTemplate = contentChild.required<TemplateRef<unknown>>('row');
}

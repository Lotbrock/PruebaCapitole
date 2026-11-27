import { Component, input } from '@angular/core';
import { ApiError } from '../../core/errors/api-error';

/** Shows an ApiError in a readable way (message + domain code). */
@Component({
  selector: 'app-error-alert',
  template: `
    @if (error(); as e) {
      <div class="alert alert-danger" role="alert">
        <strong>{{ e.title }}</strong>
        @if (e.detail) {
          <div>{{ e.detail }}</div>
        }
        @if (e.code) {
          <small class="text-body-secondary">Código: <code>{{ e.code }}</code></small>
        }
      </div>
    }
  `,
})
export class ErrorAlertComponent {
  readonly error = input<ApiError | null>(null);
}

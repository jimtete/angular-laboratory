import { Component, effect, input, signal } from '@angular/core';

interface SheetTableRow {
  id: number;
  value: string;
}

@Component({
  selector: 'app-sheet-table',
  templateUrl: './sheet-table.html',
  styleUrl: './sheet-table.css',
})
export class SheetTable {
  private nextRowId = 2;

  readonly title = input.required<string>();
  readonly resetVersion = input(0);
  protected readonly rows = signal<SheetTableRow[]>([{ id: 1, value: '' }]);

  constructor() {
    effect(() => {
      this.resetVersion();
      this.resetRows();
    });
  }

  protected addRow(): void {
    this.rows.update((rows) => [...rows, { id: this.nextRowId++, value: '' }]);
  }

  protected updateRow(rowId: number, value: string): void {
    this.rows.update((rows) =>
      rows.map((row) => (row.id === rowId ? { ...row, value } : row)),
    );
  }

  protected removeEmptyRows(): void {
    this.rows.update((rows) => {
      const populatedRows = rows.filter((row) => row.value.trim().length > 0);

      return populatedRows.length > 0 ? populatedRows : [{ id: this.nextRowId++, value: '' }];
    });
  }

  private resetRows(): void {
    this.nextRowId = 2;
    this.rows.set([{ id: 1, value: '' }]);
  }
}

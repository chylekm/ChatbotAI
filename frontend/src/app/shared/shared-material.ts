import { importProvidersFrom } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from "@angular/material/card";
import { MatDivider } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatToolbarModule } from "@angular/material/toolbar";

export const SHARED_MATERIAL_IMPORTS = [
  MatToolbarModule,
  MatCardModule,
  MatInputModule,
  MatButtonModule,
  MatIconModule,
  MatDivider
];
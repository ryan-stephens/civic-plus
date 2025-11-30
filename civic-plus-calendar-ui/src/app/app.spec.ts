import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([])],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should initialize with sidebar not collapsed', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app.isCollapsed()).toBeFalsy();
  });

  it('should toggle sidebar collapse state', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    app.isCollapsed.set(true);
    expect(app.isCollapsed()).toBeTruthy();
  });
});

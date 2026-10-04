import { Component } from 'react';

// Task 9.2 - "never crash on failure". API failures are already turned into
// messages by describeError. This catches anything else that throws while
// rendering (a bug, unexpected data) and shows a recoverable message instead
// of a blank white page.
export default class ErrorBoundary extends Component {
  constructor(props) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError() {
    return { hasError: true };
  }

  componentDidCatch(error, info) {
    console.error('Unexpected UI error', error, info.componentStack);
  }

  render() {
    if (this.state.hasError) {
      return (
        <main className="auth-page">
          <div className="card" role="alert">
            <h1>Something went wrong.</h1>
            <p>The page hit an unexpected problem. Your data on the server is safe.</p>
            <button type="button" onClick={() => this.props.onReload?.() ?? window.location.reload()}>
              Reload
            </button>
          </div>
        </main>
      );
    }

    return this.props.children;
  }
}

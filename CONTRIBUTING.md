# Contributing to Massage Booking Bot

Thank you for your interest in contributing to the Massage Booking Bot project! This document provides guidelines for contributing.

## Code of Conduct

- Be respectful and inclusive
- Follow professional standards
- Provide constructive feedback

## How to Contribute

### Reporting Bugs

1. Check if the bug has already been reported in Issues
2. Create a new issue with:
   - Clear description of the bug
   - Steps to reproduce
   - Expected vs actual behavior
   - Environment details (OS, .NET version, etc.)

### Suggesting Features

1. Check if the feature has already been suggested
2. Create a new issue with:
   - Clear description of the feature
   - Use cases
   - Potential implementation approach

### Pull Requests

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Make your changes
4. Follow the coding standards (see below)
5. Write or update tests
6. Commit your changes (`git commit -m 'Add AmazingFeature'`)
7. Push to the branch (`git push origin feature/AmazingFeature`)
8. Open a Pull Request

## Coding Standards

### C# / .NET

- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Keep methods focused and concise
- Use LINQ where appropriate

### TypeScript / Angular

- Follow [Angular Style Guide](https://angular.io/guide/styleguide)
- Use TypeScript strict mode
- Implement proper error handling
- Use reactive programming patterns (RxJS)

### General

- Write clean, readable code
- Add comments for complex logic
- Keep files focused on a single responsibility
- Follow SOLID principles
- Write unit tests for new features

## Project Structure

```
/src
  /MassageBookingBot.Domain          - Entities, Enums
  /MassageBookingBot.Application     - Business Logic, CQRS
  /MassageBookingBot.Infrastructure  - Data Access, External Services
  /MassageBookingBot.Api            - REST API
  /MassageBookingBot.BotWorker      - Telegram Bot Worker
/admin-panel                         - Angular Admin Panel
```

## Testing

- Write unit tests for new features
- Ensure all tests pass before submitting PR
- Aim for high code coverage
- Test edge cases

## Documentation

- Update README.md if needed
- Update SETUP.md for configuration changes
- Add inline comments for complex code
- Update API documentation (Swagger)

## Commit Messages

Follow conventional commits format:

```
type(scope): subject

body

footer
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes
- `refactor`: Code refactoring
- `test`: Test changes
- `chore`: Build/tooling changes

Example:
```
feat(bot): add support for appointment rescheduling

Implement functionality to allow users to reschedule their appointments
through the Telegram bot interface.

Closes #123
```

## Release Process

1. Update version numbers
2. Update CHANGELOG.md
3. Create a release tag
4. Build and test
5. Deploy to production

## Questions?

Feel free to open an issue for any questions about contributing.

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

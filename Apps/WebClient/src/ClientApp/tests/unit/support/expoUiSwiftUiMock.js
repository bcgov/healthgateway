/* global jest */
/**
 * SwiftUI presents a popover's content only while `isPresented`; the generated native view mock renders every slot at
 * once, so `Popover` here leaves out its content until it is presented.
 */
const { Children } = require("react");

const actual = jest.requireActual("@expo/ui/swift-ui");

const Trigger = ({ children }) => children;
const Content = ({ children }) => children;

function Popover({ isPresented, children }) {
    return Children.map(children, (child) =>
        child?.type === Content && !isPresented ? null : child
    );
}
Popover.Trigger = Trigger;
Popover.Content = Content;

module.exports = { ...actual, Popover };
